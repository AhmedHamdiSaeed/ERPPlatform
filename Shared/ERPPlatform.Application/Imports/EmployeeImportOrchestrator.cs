using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Internal orchestration for the import pipeline: chunk creation, scheduling and
/// stall recovery. Deliberately NOT an application service, so none of this is
/// reachable over HTTP — only the authorised <c>EmployeeImportAppService</c> surface
/// and the Hangfire jobs call it.
/// </summary>
public class EmployeeImportOrchestrator : ITransientDependency
{
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;
    private readonly IRepository<EmployeeImportChunk, Guid> _chunkRepository;
    private readonly IRepository<EmployeeImportError, Guid> _errorRepository;
    private readonly IBlobContainer<EmployeeImportContainer> _blobContainer;
    private readonly IEmployeeImportExcelReader _excelReader;
    private readonly IEmployeeImportJobScheduler _scheduler;
    private readonly EmployeeImportProgressService _progress;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly EmployeeImportOptions _options;

    public EmployeeImportOrchestrator(
        IRepository<EmployeeImportJob, Guid> jobRepository,
        IRepository<EmployeeImportChunk, Guid> chunkRepository,
        IRepository<EmployeeImportError, Guid> errorRepository,
        IBlobContainer<EmployeeImportContainer> blobContainer,
        IEmployeeImportExcelReader excelReader,
        IEmployeeImportJobScheduler scheduler,
        EmployeeImportProgressService progress,
        IUnitOfWorkManager unitOfWorkManager,
        IGuidGenerator guidGenerator,
        IClock clock,
        IOptions<EmployeeImportOptions> options)
    {
        _jobRepository = jobRepository;
        _chunkRepository = chunkRepository;
        _errorRepository = errorRepository;
        _blobContainer = blobContainer;
        _excelReader = excelReader;
        _scheduler = scheduler;
        _progress = progress;
        _unitOfWorkManager = unitOfWorkManager;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _options = options.Value;
    }

    // ── Start ───────────────────────────────────────────────────

    /// <summary>
    /// Persists the job, then splits the rows into chunks and queues the first one.
    /// Returns the persisted job so the API can reply with totalRows / totalChunks.
    /// </summary>
    public async Task<EmployeeImportJob> StartAsync(
        string fileName,
        long fileSize,
        string fileHash,
        string storageKey,
        EmployeeImportScanResult scan,
        EmployeeImportScheduleArgs scheduleArgs)
    {
        var chunkSize = _options.ChunkSize <= 0
            ? EmployeeImportConsts.DefaultChunkSize
            : Math.Min(_options.ChunkSize, EmployeeImportConsts.MaxChunkSize);

        var totalRows = scan.TotalRows;
        var totalChunks = (int)Math.Ceiling(totalRows / (double)chunkSize);

        using (var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
        {
            var job = new EmployeeImportJob
            {
                FileName = fileName,
                FileSize = fileSize,
                FileHash = fileHash,
                StorageKey = storageKey,
                Status = EmployeeImportStatus.Queued,
                TotalRows = totalRows,
                ChunkSize = chunkSize,
                TotalChunks = totalChunks,
                CompletedChunks = 0,
                CurrentChunk = totalChunks > 0 ? 1 : 0,
                CreatedByUserName = scheduleArgs.UserName,
                ScanErrorCount = scan.DuplicateErrors.Count
            };

            job = await _jobRepository.InsertAsync(job, autoSave: true);

            var chunks = new List<EmployeeImportChunk>();
            for (var n = 1; n <= totalChunks; n++)
            {
                var start = (n - 1) * chunkSize + 1;
                var end = Math.Min(n * chunkSize, totalRows);

                chunks.Add(new EmployeeImportChunk
                {
                    ImportJobId = job.Id,
                    ChunkNumber = n,
                    StartRow = start,
                    EndRow = end,
                    Status = EmployeeImportChunkStatus.Pending
                });
            }

            if (chunks.Count > 0)
            {
                await _chunkRepository.InsertManyAsync(chunks, autoSave: true);
            }

            // Up-front duplicate findings belong to the job, not to a chunk.
            var scanErrors = scan.DuplicateErrors.Select(e => new EmployeeImportError
            {
                ImportJobId = job.Id,
                ChunkId = null,
                RowNumber = e.RowNumber,
                ColumnName = e.ColumnName,
                Value = Truncate(e.Value, EmployeeImportConsts.MaxErrorValueLength),
                ErrorMessage = Truncate(e.ErrorMessage, EmployeeImportConsts.MaxErrorMessageLength),
                CreatedAt = _clock.Now
            }).ToList();

            if (scanErrors.Count > 0)
            {
                await _errorRepository.InsertManyAsync(scanErrors, autoSave: true);
            }

            await uow.CompleteAsync();

            scheduleArgs.ImportJobId = job.Id;
            return job;
        }
    }

    /// <summary>Queues the first chunk of a freshly created job.</summary>
    public async Task QueueFirstChunkAsync(EmployeeImportScheduleArgs args)
    {
        if (args.ImportJobId == Guid.Empty) return;

        await _scheduler.EnqueueChunkAsync(args, 1);
    }

    // ── Retry ───────────────────────────────────────────────────

    /// <summary>
    /// Re-queues only the chunks that are not Completed. Completed chunks keep their
    /// status, so a retry continues from the first incomplete chunk instead of row 1.
    /// </summary>
    public async Task<bool> ResumeAsync(Guid importJobId, EmployeeImportScheduleArgs args, bool resetFailedChunks = true)
    {
        using (var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
        {
            var job = await _jobRepository.FindAsync(importJobId);
            if (job == null)
            {
                await uow.CompleteAsync();
                return false;
            }

            if (job.Status == EmployeeImportStatus.Completed ||
                job.Status == EmployeeImportStatus.CompletedWithErrors)
            {
                await uow.CompleteAsync();
                return false;
            }

            var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == importJobId);

            foreach (var chunk in chunks.Where(c =>
                         c.Status == EmployeeImportChunkStatus.Failed ||
                         c.Status == EmployeeImportChunkStatus.Cancelled))
            {
                if (resetFailedChunks)
                {
                    chunk.Status = EmployeeImportChunkStatus.Pending;
                    chunk.LastError = null;
                    chunk.CompletedAt = null;
                    await _chunkRepository.UpdateAsync(chunk);
                }
            }

            job.Status = EmployeeImportStatus.Processing;
            job.CompletedAt = null;
            job.LastError = null;

            await _jobRepository.UpdateAsync(job, autoSave: true);
            await uow.CompleteAsync();
        }

        var next = await _progress.GetNextPendingChunkAsync(importJobId);
        if (next == null)
        {
            // Nothing left to do — the counters may already be final.
            await _progress.RecomputeAsync(importJobId);
            return false;
        }

        args.ImportJobId = importJobId;
        await _scheduler.EnqueueChunkAsync(args, next.Value);
        return true;
    }

    // ── Cancel ──────────────────────────────────────────────────

    /// <summary>
    /// Persists cancellation. Chunks already committed stay committed; chunks that are
    /// merely Pending are marked Cancelled so a queued Hangfire job will skip them.
    /// A chunk that is mid-flight when Cancel lands is not interrupted — it finishes,
    /// and only then sees the cancelled job.
    /// </summary>
    public async Task CancelAsync(Guid importJobId)
    {
        using (var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
        {
            var job = await _jobRepository.GetAsync(importJobId);
            job.Status = EmployeeImportStatus.Cancelled;
            job.CancelledAt = _clock.Now;
            job.CompletedAt = _clock.Now;
            job.CurrentChunk = 0;

            var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == importJobId);
            foreach (var chunk in chunks.Where(c => c.Status == EmployeeImportChunkStatus.Pending))
            {
                chunk.Status = EmployeeImportChunkStatus.Cancelled;
                chunk.CompletedAt = _clock.Now;
                await _chunkRepository.UpdateAsync(chunk);
            }

            await _jobRepository.UpdateAsync(job, autoSave: true);
            await uow.CompleteAsync();
        }

        await _progress.RecomputeAsync(importJobId);
        await _progress.FinishAsync(importJobId, EmployeeImportStatus.Cancelled, null);
    }

    // ── Recovery ────────────────────────────────────────────────

    /// <summary>
    /// Watchdog. After an application restart (or a crashed worker) a chunk can be
    /// left in Processing forever. This releases it and re-queues the first
    /// incomplete chunk of every still-active job. Safe to run concurrently: the
    /// atomic claim in the chunk processor decides who actually runs a chunk.
    /// </summary>
    public async Task<int> RecoverStalledJobsAsync(IEmployeeImportScheduleArgsFactory argsFactory)
    {
        var recovered = 0;
        var cutoff = _clock.Now.AddMinutes(-Math.Max(1, _options.StuckChunkTimeoutMinutes));

        List<Guid> activeJobIds;
        using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
        {
            activeJobIds = (await _jobRepository.GetListAsync(j =>
                    j.Status == EmployeeImportStatus.Queued || j.Status == EmployeeImportStatus.Processing))
                .Select(j => j.Id)
                .ToList();
            await uow.CompleteAsync();
        }

        foreach (var jobId in activeJobIds)
        {
            using (var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
            {
                var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == jobId);

                var stuck = chunks.Where(c =>
                        c.Status == EmployeeImportChunkStatus.Processing &&
                        c.StartedAt.HasValue && c.StartedAt.Value < cutoff)
                    .ToList();

                foreach (var chunk in stuck)
                {
                    chunk.Status = EmployeeImportChunkStatus.Pending;
                    chunk.LastError = "Released by the recovery watchdog: the previous attempt did not report back.";
                    await _chunkRepository.UpdateAsync(chunk);
                }

                await uow.CompleteAsync();
            }

            var next = await _progress.GetNextPendingChunkAsync(jobId);
            if (next != null)
            {
                var args = await argsFactory.CreateAsync(jobId);
                if (args != null)
                {
                    await _scheduler.EnqueueChunkAsync(args, next.Value);
                    recovered++;
                }
            }
            else
            {
                await _progress.RecomputeAsync(jobId);
            }
        }

        return recovered;
    }

    // ── Cleanup ─────────────────────────────────────────────────

    /// <summary>
    /// Removes retained source files once an import is finished and the retention
    /// window has passed. History rows are never touched — only the spreadsheet.
    /// </summary>
    public async Task<int> CleanupExpiredSourceFilesAsync()
    {
        var cutoff = _clock.Now.AddDays(-Math.Max(1, _options.SourceFileRetentionDays));

        List<EmployeeImportJob> finished;
        using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
        {
            finished = (await _jobRepository.GetListAsync(j => j.CompletedAt != null && j.CompletedAt < cutoff))
                .Where(j => !string.IsNullOrWhiteSpace(j.StorageKey))
                .ToList();
            await uow.CompleteAsync();
        }

        var deleted = 0;
        foreach (var job in finished)
        {
            // Keep the source file while a cancelled import could still be resumed.
            if (job.Status == EmployeeImportStatus.Cancelled && job.FailedChunks == 0)
            {
                continue;
            }

            try
            {
                if (await _blobContainer.ExistsAsync(job.StorageKey))
                {
                    await _blobContainer.DeleteAsync(job.StorageKey);
                }

                job.StorageKey = string.Empty;
                await _jobRepository.UpdateAsync(job, autoSave: true);
                deleted++;
            }
            catch
            {
                // A missing or locked blob must never break the cleanup sweep.
            }
        }

        return deleted;
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed.Substring(0, max);
    }
}

/// <summary>
/// Builds the scheduler arguments (tenant + user context) for a job, so background
/// workers run as the person who started the import.
/// </summary>
public interface IEmployeeImportScheduleArgsFactory
{
    Task<EmployeeImportScheduleArgs?> CreateAsync(Guid importJobId);
}
