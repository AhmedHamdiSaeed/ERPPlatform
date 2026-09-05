using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Owns every write to <c>EmployeeImportJob</c>'s counters and status.
///
/// Counters are *recomputed* from the chunk rows rather than incremented, so a
/// retried chunk cannot double-count and a partially applied update self-heals on
/// the next chunk. This is what makes the numbers correct after a crash.
/// </summary>
public class EmployeeImportProgressService : ITransientDependency
{
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;
    private readonly IRepository<EmployeeImportChunk, Guid> _chunkRepository;
    private readonly IRepository<EmployeeImportError, Guid> _errorRepository;
    private readonly IEmployeeImportNotifier _notifier;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IClock _clock;

    public EmployeeImportProgressService(
        IRepository<EmployeeImportJob, Guid> jobRepository,
        IRepository<EmployeeImportChunk, Guid> chunkRepository,
        IRepository<EmployeeImportError, Guid> errorRepository,
        IEmployeeImportNotifier notifier,
        IUnitOfWorkManager unitOfWorkManager,
        IClock clock)
    {
        _jobRepository = jobRepository;
        _chunkRepository = chunkRepository;
        _errorRepository = errorRepository;
        _notifier = notifier;
        _unitOfWorkManager = unitOfWorkManager;
        _clock = clock;
    }

    /// <summary>1-based number of the first chunk that still needs work, or null when everything is done.</summary>
    public async Task<int?> GetNextPendingChunkAsync(Guid importJobId)
    {
        var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == importJobId);
        if (chunks.Count == 0) return null;

        var pending = chunks
            .Where(c => c.Status is EmployeeImportChunkStatus.Pending or EmployeeImportChunkStatus.Processing)
            .OrderBy(c => c.ChunkNumber)
            .Select(c => c.ChunkNumber)
            .ToList();

        return pending.Count == 0 ? null : pending[0];
    }

    /// <summary>
    /// Recomputes counters from persisted chunks and, when every chunk has reached a
    /// terminal state, closes the job and pushes the one and only completion
    /// notification. Runs in its own transaction so partial progress never blocks it.
    /// </summary>
    public async Task<EmployeeImportJob> RecomputeAsync(Guid importJobId)
    {
        using var uow = _unitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

        var job = await _jobRepository.GetAsync(importJobId);
        var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == importJobId);

        job.SuccessfulRows = chunks.Sum(c => c.SuccessfulRows);
        job.FailedRows = chunks.Sum(c => c.FailedRows);
        job.ProcessedRows = job.SuccessfulRows + job.FailedRows + job.ScanErrorCount;
        job.CompletedChunks = chunks.Count(c => c.Status == EmployeeImportChunkStatus.Completed);
        job.FailedChunks = chunks.Count(c => c.Status == EmployeeImportChunkStatus.Failed);
        job.TotalChunks = chunks.Count;

        var inFlight = chunks.FirstOrDefault(c => c.Status == EmployeeImportChunkStatus.Processing);
        job.CurrentChunk = inFlight?.ChunkNumber
                           ?? chunks.Where(c => c.Status == EmployeeImportChunkStatus.Pending)
                               .OrderBy(c => c.ChunkNumber)
                               .Select(c => (int?)c.ChunkNumber)
                               .FirstOrDefault()
                           ?? 0;

        if (job.IsFinished)
        {
            await uow.SaveChangesAsync();
            await uow.CompleteAsync();
            return job;
        }

        if (job.Status == EmployeeImportStatus.Processing && job.StartedAt == null)
        {
            job.StartedAt = _clock.Now;
        }

        var allDone = chunks.Count > 0 && chunks.All(c =>
            c.Status is EmployeeImportChunkStatus.Completed
                or EmployeeImportChunkStatus.Failed
                or EmployeeImportChunkStatus.Cancelled);

        if (allDone)
        {
            job.CurrentChunk = 0;
            job.CompletedAt = _clock.Now;
            job.LastError = chunks
                .Where(c => !string.IsNullOrWhiteSpace(c.LastError))
                .OrderByDescending(c => c.ChunkNumber)
                .Select(c => c.LastError)
                .FirstOrDefault();

            job.Status = job.FailedChunks > 0
                ? EmployeeImportStatus.Failed
                : (job.FailedRows > 0 || job.ScanErrorCount > 0
                    ? EmployeeImportStatus.CompletedWithErrors
                    : EmployeeImportStatus.Completed);
        }
        else
        {
            // No chunk is in flight and none is pending only while chunks are still
            // being claimed; keep Processing so the UI does not flicker.
            job.Status = EmployeeImportStatus.Processing;
        }

        await _jobRepository.UpdateAsync(job, autoSave: true);
        await uow.CompleteAsync();

        if (allDone)
        {
            await NotifyCompletedAsync(job);
        }

        return job;
    }

    /// <summary>
    /// Closes the job with an explicit status. Used by Cancel (Cancelled) and by the
    /// chunk processor when a chunk exhausts its retries (Failed).
    /// </summary>
    public async Task<EmployeeImportJob> FinishAsync(Guid importJobId, EmployeeImportStatus status, string? error)
    {
        using var uow = _unitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

        var job = await _jobRepository.GetAsync(importJobId);
        if (job.IsFinished && status != EmployeeImportStatus.Cancelled)
        {
            await uow.CompleteAsync();
            return job;
        }

        job.Status = status;
        job.CompletedAt = _clock.Now;
        job.CurrentChunk = 0;
        if (!string.IsNullOrWhiteSpace(error))
        {
            job.LastError = Truncate(error, EmployeeImportConsts.MaxErrorMessageLength);
        }

        await _jobRepository.UpdateAsync(job, autoSave: true);
        await uow.CompleteAsync();

        await NotifyCompletedAsync(job);
        return job;
    }

    public async Task NotifyProgressAsync(EmployeeImportJob job)
    {
        var payload = BuildPayload(job, EmployeeImportNotificationTypes.Progress, null);
        if (string.IsNullOrWhiteSpace(job.CreatorId?.ToString())) return;

        try
        {
            await _notifier.NotifyProgressAsync(job.CreatorId!.Value.ToString(), payload);
        }
        catch
        {
            // Real-time updates are best effort; the database stays authoritative and
            // the client recovers by polling the status endpoint.
        }
    }

    private async Task NotifyCompletedAsync(EmployeeImportJob job)
    {
        if (job.CreatorId == null) return;

        var message = BuildCompletedMessage(job);
        var payload = BuildPayload(job, EmployeeImportNotificationTypes.Completed, message);

        try
        {
            await _notifier.NotifyCompletedAsync(job.CreatorId.Value.ToString(), payload);
        }
        catch
        {
            // Never let a broken transport fail the import.
        }
    }

    private static string BuildCompletedMessage(EmployeeImportJob job)
    {
        return job.Status switch
        {
            EmployeeImportStatus.Completed =>
                $"Employee import completed successfully. {job.SuccessfulRows:N0} employees imported.",
            EmployeeImportStatus.CompletedWithErrors =>
                $"Employee import completed with errors. {job.SuccessfulRows:N0} employees imported successfully and {job.FailedRows:N0} rows failed.",
            EmployeeImportStatus.Failed =>
                "Employee import failed. Please check the import details and try again.",
            EmployeeImportStatus.Cancelled =>
                $"Employee import was cancelled. {job.SuccessfulRows:N0} employees imported before cancellation.",
            _ => $"Employee import status: {job.Status}."
        };
    }

    private static EmployeeImportNotificationDto BuildPayload(EmployeeImportJob job, string type, string? message)
    {
        return new EmployeeImportNotificationDto
        {
            Type = type,
            ImportJobId = job.Id,
            Status = job.Status.ToString(),
            TotalRows = job.TotalRows,
            ProcessedRows = job.ProcessedRows,
            SuccessfulRows = job.SuccessfulRows,
            FailedRows = job.FailedRows,
            TotalChunks = job.TotalChunks,
            CompletedChunks = job.CompletedChunks,
            CurrentChunk = job.CurrentChunk,
            ProgressPercentage = job.ProgressPercentage,
            Message = message
        };
    }

    /// <summary>Deletes row-level errors produced by a previous attempt of this chunk.</summary>
    public async Task ClearChunkErrorsAsync(Guid importJobId, Guid chunkId)
    {
        using var uow = _unitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

        var stale = await _errorRepository.GetListAsync(e => e.ImportJobId == importJobId && e.ChunkId == chunkId);
        if (stale.Count > 0)
        {
            await _errorRepository.DeleteManyAsync(stale, autoSave: true);
        }

        await uow.CompleteAsync();
    }

    private static string Truncate(string value, int max)
    {
        return value.Length <= max ? value : value.Substring(0, max);
    }
}

public static class EmployeeImportNotificationTypes
{
    public const string Progress = "EmployeeImportProgress";
    public const string Completed = "EmployeeImportCompleted";
}
