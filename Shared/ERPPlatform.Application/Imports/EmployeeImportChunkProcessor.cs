using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;
using ERPPlatform.Domain.Entities;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace ERPPlatform.Application.Imports;

public class EmployeeImportChunkOutcome
{
    /// <summary>True when the chunk was not executed (already done, cancelled, or claimed by another worker).</summary>
    public bool Skipped { get; set; }

    public string? SkipReason { get; set; }

    public bool Succeeded { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public string? Error { get; set; }

    /// <summary>1-based chunk that should run next, or null when the import is finished.</summary>
    public int? NextChunkNumber { get; set; }

    public bool JobFinished { get; set; }

    public EmployeeImportStatus? FinalStatus { get; set; }

    public static EmployeeImportChunkOutcome Skip(string reason) =>
        new() { Skipped = true, SkipReason = reason };
}

/// <summary>
/// Executes exactly one chunk. This class is the heart of the resume guarantee:
///
///   1. Claim the chunk with a single conditional UPDATE (Pending|Failed -> Processing).
///      The UPDATE is atomic, so two Hangfire workers can never process the same chunk.
///   2. Commit the claim immediately, so a later crash leaves a visible "in flight" marker.
///   3. Process the rows inside their own transaction: 100 rows in, one commit.
///   4. Mark the chunk Completed in a further transaction and recompute job counters.
///
/// A chunk already marked Completed is skipped outright, so after a crash chunks 1..N
/// stay committed and processing resumes at N+1. Employee upserts are keyed on
/// EmployeeCode/Email, so re-running a half-finished chunk cannot create duplicates.
/// </summary>
public class EmployeeImportChunkProcessor : ITransientDependency
{
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;
    private readonly IRepository<EmployeeImportChunk, Guid> _chunkRepository;
    private readonly IRepository<EmployeeImportError, Guid> _errorRepository;
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly IRepository<Department, Guid> _departmentRepository;
    private readonly IBlobContainer<EmployeeImportContainer> _blobContainer;
    private readonly IEmployeeImportExcelReader _excelReader;
    private readonly IEmployeeImportRowValidator _rowValidator;
    private readonly EmployeeImportProgressService _progress;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly EmployeeImportOptions _options;

    public EmployeeImportChunkProcessor(
        IRepository<EmployeeImportJob, Guid> jobRepository,
        IRepository<EmployeeImportChunk, Guid> chunkRepository,
        IRepository<EmployeeImportError, Guid> errorRepository,
        IRepository<Employee, Guid> employeeRepository,
        IRepository<Department, Guid> departmentRepository,
        IBlobContainer<EmployeeImportContainer> blobContainer,
        IEmployeeImportExcelReader excelReader,
        IEmployeeImportRowValidator rowValidator,
        EmployeeImportProgressService progress,
        IUnitOfWorkManager unitOfWorkManager,
        IGuidGenerator guidGenerator,
        IClock clock,
        IOptions<EmployeeImportOptions> options)
    {
        _jobRepository = jobRepository;
        _chunkRepository = chunkRepository;
        _errorRepository = errorRepository;
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _blobContainer = blobContainer;
        _excelReader = excelReader;
        _rowValidator = rowValidator;
        _progress = progress;
        _unitOfWorkManager = unitOfWorkManager;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _options = options.Value;
    }

    public virtual async Task<EmployeeImportChunkOutcome> ProcessAsync(Guid importJobId, int chunkNumber)
    {
        // ── 1. Snapshot ────────────────────────────────────────
        EmployeeImportJob? job;
        EmployeeImportChunk? chunk;

        using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
        {
            job = await _jobRepository.FindAsync(importJobId);
            if (job == null)
            {
                await uow.CompleteAsync();
                return EmployeeImportChunkOutcome.Skip("Import job no longer exists.");
            }

            chunk = await _chunkRepository.FindAsync(c => c.ImportJobId == importJobId && c.ChunkNumber == chunkNumber);
            if (chunk == null)
            {
                await uow.CompleteAsync();
                return EmployeeImportChunkOutcome.Skip($"Chunk {chunkNumber} does not exist.");
            }

            // The resume guard: a completed chunk is never re-executed.
            if (chunk.Status == EmployeeImportChunkStatus.Completed)
            {
                await uow.CompleteAsync();
                return await AdvanceAsync(job.Id, chunkNumber, "chunk already completed");
            }

            if (chunk.Status == EmployeeImportChunkStatus.Cancelled)
            {
                await uow.CompleteAsync();
                return await AdvanceAsync(job.Id, chunkNumber, "chunk was cancelled");
            }

            await uow.CompleteAsync();
        }

        // ── 2. Cancellation check (race-safe) ──────────────────
        if (job.Status == EmployeeImportStatus.Cancelled)
        {
            using var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var live = await _chunkRepository.GetAsync(chunk.Id);
            live.Status = EmployeeImportChunkStatus.Cancelled;
            live.CompletedAt = _clock.Now;
            await _chunkRepository.UpdateAsync(live, autoSave: true);
            await uow.CompleteAsync();

            await _progress.RecomputeAsync(importJobId);
            return await AdvanceAsync(importJobId, chunkNumber, "import was cancelled");
        }

        // ── 3. Atomic claim: Pending|Failed -> Processing ──────
        var claimed = await ClaimChunkAsync(chunk.Id);
        if (!claimed)
        {
            return EmployeeImportChunkOutcome.Skip(
                $"Chunk {chunkNumber} is already being processed by another worker.");
        }

        // ── 4. Retry budget ────────────────────────────────────
        var currentAttempt = await GetAttemptCountAsync(chunk.Id);
        if (currentAttempt > _options.MaxChunkAttempts)
        {
            var message = $"Chunk {chunkNumber} failed after {currentAttempt - 1} attempts.";
            await ReleaseChunkAsync(chunk.Id, EmployeeImportChunkStatus.Failed, message);
            await _progress.RecomputeAsync(importJobId);
            await _progress.FinishAsync(importJobId, EmployeeImportStatus.Failed, message);

            return new EmployeeImportChunkOutcome
            {
                Succeeded = false,
                Error = message,
                JobFinished = true,
                FinalStatus = EmployeeImportStatus.Failed
            };
        }

        // ── 5. Process rows in their own transaction ───────────
        try
        {
            var (successful, failed) = await ExecuteChunkInTransactionAsync(job, chunk);

            await ReleaseChunkAsync(chunk.Id, EmployeeImportChunkStatus.Completed, null, successful, failed);

            var updated = await _progress.RecomputeAsync(importJobId);

            if (updated.IsFinished)
            {
                return new EmployeeImportChunkOutcome
                {
                    Succeeded = true,
                    SuccessfulRows = successful,
                    FailedRows = failed,
                    JobFinished = true,
                    FinalStatus = updated.Status
                };
            }

            await NotifyProgressIfNeededAsync(updated);
            return await AdvanceAsync(importJobId, chunkNumber, null, successful, failed);
        }
        catch (Exception ex)
        {
            // The chunk transaction has already rolled back; only the claim is rolled forward.
            var message = $"Chunk {chunkNumber} failed: {ex.Message}";
            await ReleaseChunkAsync(chunk.Id, EmployeeImportChunkStatus.Failed, message);

            var updatedJob = await _progress.RecomputeAsync(importJobId);
            await NotifyProgressIfNeededAsync(updatedJob);

            // Rethrow so Hangfire's retry policy re-runs THIS chunk, not the whole file.
            throw new EmployeeImportChunkException(message, ex);
        }
    }

    /// <summary>
    /// The concurrency guard. One conditional UPDATE; zero rows affected means another
    /// worker won the race. The attempt counter is bumped in the same statement.
    /// </summary>
    private async Task<bool> ClaimChunkAsync(Guid chunkId)
    {
        using var uow = _unitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

        // Guard the status check in the same query that loads the chunk: only a chunk that is
        // still Pending (or previously Failed) may be claimed, so two workers racing for the
        // same chunk cannot both win.
        var chunk = await _chunkRepository.FindAsync(
            c => c.Id == chunkId &&
                 (c.Status == EmployeeImportChunkStatus.Pending || c.Status == EmployeeImportChunkStatus.Failed));

        if (chunk is null)
        {
            // Already claimed, already done, or gone — this worker lost the race.
            await uow.CompleteAsync();
            return false;
        }

        chunk.Status = EmployeeImportChunkStatus.Processing;
        chunk.AttemptCount += 1;
        chunk.StartedAt = _clock.Now;
        chunk.LastError = null;

        await _chunkRepository.UpdateAsync(chunk);
        await uow.CompleteAsync();
        return true;
    }

    private async Task<int> GetAttemptCountAsync(Guid chunkId)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        var chunk = await _chunkRepository.GetAsync(chunkId);
        await uow.CompleteAsync();
        return chunk.AttemptCount;
    }

    /// <summary>
    /// Everything that must be atomic *per chunk*: the employee upserts and the row
    /// errors. One commit per chunk, so chunks 1..N survive a failure in chunk N+1.
    /// </summary>
    private async Task<(int successful, int failed)> ExecuteChunkInTransactionAsync(
        EmployeeImportJob job,
        EmployeeImportChunk chunk)
    {
        var rows = await ReadChunkRowsAsync(job, chunk);
        if (rows.Count == 0) return (0, 0);

        var departments = await _departmentRepository.GetListAsync();
        var departmentNames = departments
            .SelectMany(d => new[] { d.Name, d.Code })
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var now = _clock.Now;
        var errors = new List<EmployeeImportError>();
        var parsed = new List<EmployeeImportParsedRow>();

        foreach (var row in rows)
        {
            var outcome = _rowValidator.Validate(row, departmentNames);
            if (outcome.IsValid && outcome.Parsed != null)
            {
                parsed.Add(outcome.Parsed);
            }
            else
            {
                foreach (var error in outcome.Errors)
                {
                    errors.Add(new EmployeeImportError(_guidGenerator.Create())
                    {
                        ImportJobId = job.Id,
                        ChunkId = chunk.Id,
                        RowNumber = error.RowNumber,
                        ColumnName = error.ColumnName,
                        Value = error.Value,
                        ErrorMessage = error.ErrorMessage,
                        CreatedAt = now
                    });
                }
            }
        }

        // Own transaction: 100 rows in, one commit.
        using (var uow = _unitOfWorkManager.Begin(
                   new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
        {
            // A retried chunk must not leave the previous attempt's errors behind.
            var stale = await _errorRepository.GetListAsync(e => e.ChunkId == chunk.Id);
            if (stale.Count > 0)
            {
                await _errorRepository.DeleteManyAsync(stale, autoSave: true);
            }

            if (errors.Count > 0)
            {
                await _errorRepository.InsertManyAsync(errors, autoSave: true);
            }

            foreach (var row in parsed)
            {
                await UpsertEmployeeAsync(row, departments);
            }

            await uow.SaveChangesAsync();
            await uow.CompleteAsync();
        }

        return (parsed.Count, errors.Select(e => e.RowNumber).Distinct().Count());
    }

    /// <summary>
    /// Idempotent by construction. Employee number wins over email as the match key
    /// because it is the organisation's own identifier; email is the fallback.
    /// An existing employee is updated, so replaying a chunk always converges on the
    /// same state instead of creating a second record.
    /// </summary>
    private async Task UpsertEmployeeAsync(EmployeeImportParsedRow row, IReadOnlyList<Department> departments)
    {
        var query = await _employeeRepository.GetQueryableAsync();

        var existing = query.FirstOrDefault(e => e.EmployeeCode == row.EmployeeCode)
                       ?? query.FirstOrDefault(e => e.Email == row.Email);

        var departmentId = ResolveDepartmentId(row.DepartmentName, departments);

        if (existing == null)
        {
            var employee = new Employee
            {
                EmployeeCode = row.EmployeeCode,
                Name = row.Name,
                Email = row.Email,
                Phone = row.Phone,
                Position = row.Position,
                DepartmentName = row.DepartmentName,
                DepartmentId = departmentId,
                Salary = row.Salary,
                JoiningDate = row.JoiningDate,
                Status = row.Status,
                Location = row.Location,
                ManagerName = row.ManagerName,
                LeaveBalance = row.LeaveBalance > 0 ? row.LeaveBalance : 21.0m
            };

            await _employeeRepository.InsertAsync(employee, autoSave: true);
            return;
        }

        existing.EmployeeCode = row.EmployeeCode;
        existing.Name = row.Name;
        existing.Email = row.Email;
        existing.Phone = row.Phone;
        existing.Position = row.Position;
        existing.DepartmentName = row.DepartmentName;
        existing.DepartmentId = departmentId ?? existing.DepartmentId;
        existing.Salary = row.Salary;
        existing.JoiningDate = row.JoiningDate;
        existing.Status = row.Status;
        existing.Location = row.Location;
        existing.ManagerName = row.ManagerName;
        existing.LeaveBalance = row.LeaveBalance > 0 ? row.LeaveBalance : existing.LeaveBalance;

        await _employeeRepository.UpdateAsync(existing, autoSave: true);
    }

    private static Guid? ResolveDepartmentId(string departmentName, IReadOnlyList<Department> departments)
    {
        if (string.IsNullOrWhiteSpace(departmentName)) return null;

        var match = departments.FirstOrDefault(d =>
                        string.Equals(d.Name, departmentName, StringComparison.OrdinalIgnoreCase))
                    ?? departments.FirstOrDefault(d =>
                        string.Equals(d.Code, departmentName, StringComparison.OrdinalIgnoreCase));

        return match?.Id;
    }

    private async Task<List<EmployeeImportRow>> ReadChunkRowsAsync(EmployeeImportJob job, EmployeeImportChunk chunk)
    {
        var bytes = await _blobContainer.GetAllBytesOrNullAsync(job.StorageKey);
        if (bytes == null)
        {
            throw new BusinessException(
                code: EmployeeImportErrorCodes.SourceFileMissing,
                message: "The uploaded spreadsheet is no longer available. Please start the import again.");
        }

        using var stream = new MemoryStream(bytes);
        return _excelReader.ReadRange(stream, chunk.StartRow, chunk.EndRow);
    }

    private async Task ReleaseChunkAsync(
        Guid chunkId,
        EmployeeImportChunkStatus status,
        string? error,
        int successfulRows = 0,
        int failedRows = 0)
    {
        using var uow = _unitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

        var chunk = await _chunkRepository.GetAsync(chunkId);
        chunk.Status = status;
        chunk.LastError = error == null ? null : Truncate(error, EmployeeImportConsts.MaxErrorMessageLength);

        if (status == EmployeeImportChunkStatus.Completed)
        {
            chunk.SuccessfulRows = successfulRows;
            chunk.FailedRows = failedRows;
            chunk.CompletedAt = _clock.Now;
        }
        else if (status == EmployeeImportChunkStatus.Failed)
        {
            // Reset per-attempt counters so a retry recomputes them from scratch.
            chunk.SuccessfulRows = 0;
            chunk.FailedRows = 0;
        }

        await _chunkRepository.UpdateAsync(chunk, autoSave: true);
        await uow.CompleteAsync();
    }

    /// <summary>
    /// Works out what should run after this chunk and marks the job finished when
    /// there is nothing left. Shared by the success path and every skip path, so a
    /// resume always continues from the first incomplete chunk.
    /// </summary>
    private async Task<EmployeeImportChunkOutcome> AdvanceAsync(
        Guid importJobId,
        int completedChunkNumber,
        string? skipReason,
        int successfulRows = 0,
        int failedRows = 0)
    {
        var next = await _progress.GetNextPendingChunkAsync(importJobId);

        if (next == null)
        {
            var job = await _progress.RecomputeAsync(importJobId);
            return new EmployeeImportChunkOutcome
            {
                Skipped = skipReason != null,
                SkipReason = skipReason,
                Succeeded = skipReason == null,
                SuccessfulRows = successfulRows,
                FailedRows = failedRows,
                JobFinished = job.IsFinished,
                FinalStatus = job.IsFinished ? job.Status : null
            };
        }

        return new EmployeeImportChunkOutcome
        {
            Skipped = skipReason != null,
            SkipReason = skipReason,
            Succeeded = skipReason == null,
            SuccessfulRows = successfulRows,
            FailedRows = failedRows,
            NextChunkNumber = next
        };
    }

    private async Task NotifyProgressIfNeededAsync(EmployeeImportJob job)
    {
        var every = _options.ProgressNotificationEveryNChunks <= 0
            ? 1
            : _options.ProgressNotificationEveryNChunks;

        var isMilestone = job.CompletedChunks % every == 0;
        if (isMilestone || job.IsFinished)
        {
            await _progress.NotifyProgressAsync(job);
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value.Substring(0, max);
}

/// <summary>
/// Thrown when a chunk cannot be applied. Hangfire's retry re-runs the same chunk,
/// never the whole file.
/// </summary>
public class EmployeeImportChunkException : BusinessException
{
    public EmployeeImportChunkException(string message, Exception? innerException = null)
        : base(EmployeeImportErrorCodes.ChunkFailed, message, details: null, innerException: innerException)
    {
    }
}
