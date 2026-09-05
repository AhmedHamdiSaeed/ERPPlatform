using System;
using ERPPlatform.Imports;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Domain.Imports;

/// <summary>
/// One row per uploaded Excel file. Lives in the database so an import survives
/// application restarts, Hangfire retries and browser refreshes.
/// </summary>
public class EmployeeImportJob : FullAuditedAggregateRoot<Guid>
{
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    /// <summary>SHA-256 of the uploaded bytes; used to refuse a duplicate submission of an identical file.</summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>Blob-container key of the retained source file (needed until every chunk is done).</summary>
    public string StorageKey { get; set; } = string.Empty;

    public EmployeeImportStatus Status { get; set; } = EmployeeImportStatus.Queued;

    public int TotalRows { get; set; }

    public int ProcessedRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    /// <summary>
    /// Rows rejected by the up-front structural scan (in-file duplicates). They have
    /// no chunk of their own, so they are tracked separately to keep the counters honest.
    /// </summary>
    public int ScanErrorCount { get; set; }

    public int ChunkSize { get; set; }

    public int TotalChunks { get; set; }

    public int CompletedChunks { get; set; }

    public int FailedChunks { get; set; }

    /// <summary>1-based number of the chunk currently being processed; 0 when idle.</summary>
    public int CurrentChunk { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? LastError { get; set; }

    /// <summary>Denormalised so the history page can filter/display it without joining Identity.</summary>
    public string CreatedByUserName { get; set; } = string.Empty;

    public DateTime? CancelledAt { get; set; }

    /// <summary>How many times an administrator has re-queued incomplete chunks.</summary>
    public int RetryCount { get; set; }

    // ── Derived helpers (never persisted) ────────────────────────

    /// <summary>0-100, computed from persisted counters rather than from frontend state.</summary>
    public int ProgressPercentage =>
        TotalChunks <= 0 ? 0 : (int)System.Math.Round(CompletedChunks * 100.0 / TotalChunks);

    /// <summary>True while a worker may still pick up chunks.</summary>
    public bool IsActive =>
        Status is EmployeeImportStatus.Queued or EmployeeImportStatus.Processing;

    /// <summary>True once the job reached a terminal state.</summary>
    public bool IsFinished =>
        Status is EmployeeImportStatus.Completed
            or EmployeeImportStatus.CompletedWithErrors
            or EmployeeImportStatus.Failed
            or EmployeeImportStatus.Cancelled;
}
