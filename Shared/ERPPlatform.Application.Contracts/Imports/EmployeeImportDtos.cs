using System;
using System.Collections.Generic;
using ERPPlatform.Imports;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Upload payload. <see cref="File"/> is bound from <c>multipart/form-data</c>.
/// </summary>
public class EmployeeImportInput
{
    public IRemoteStreamContent File { get; set; } = default!;
}

/// <summary>
/// Returned immediately by the upload endpoint. The browser must NOT wait for the
/// rows to be created — it only gets the identifier it needs to follow progress.
/// </summary>
public class EmployeeImportStartResultDto
{
    public Guid ImportJobId { get; set; }
    public string Status { get; set; } = nameof(EmployeeImportStatus.Queued);
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int TotalRows { get; set; }
    public int TotalChunks { get; set; }
    public int ChunkSize { get; set; }
}

/// <summary>Lightweight projection used by the history table.</summary>
public class EmployeeImportJobDto : EntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public EmployeeImportStatus Status { get; set; }
    public string StatusText => Status.ToString();

    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }

    public int ChunkSize { get; set; }
    public int TotalChunks { get; set; }
    public int CompletedChunks { get; set; }
    public int FailedChunks { get; set; }
    public int CurrentChunk { get; set; }

    public int ProgressPercentage { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreationTime { get; set; }

    public string CreatedByUserName { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }

    /// <summary>Convenience flag so the UI can offer Retry without a second query.</summary>
    public bool CanRetry { get; set; }

    /// <summary>Convenience flag so the UI can offer Cancel without a second query.</summary>
    public bool CanCancel { get; set; }
}

/// <summary>Row-level snapshot of a chunk — the core of the resume story made visible.</summary>
public class EmployeeImportChunkDto : EntityDto<Guid>
{
    public Guid ImportJobId { get; set; }
    public int ChunkNumber { get; set; }
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public int RowCount { get; set; }
    public EmployeeImportChunkStatus Status { get; set; }
    public string StatusText => Status.ToString();
    public int AttemptCount { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string? LastError { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>One rejected row.</summary>
public class EmployeeImportErrorDto : EntityDto<Guid>
{
    public Guid ImportJobId { get; set; }
    public Guid? ChunkId { get; set; }
    public int RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public string? Value { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Everything the details drawer needs in one round trip.</summary>
public class EmployeeImportDetailsDto : EmployeeImportJobDto
{
    public List<EmployeeImportChunkDto> Chunks { get; set; } = new();
}

/// <summary>Server-side filtered, paged history query.</summary>
public class EmployeeImportJobListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Matches the file name (substring, case-insensitive).</summary>
    public string? Search { get; set; }

    /// <summary>Free-text file-name filter kept separate from <see cref="Search"/> for the history page's two inputs.</summary>
    public string? FileName { get; set; }

    public EmployeeImportStatus? Status { get; set; }

    /// <summary>Matches the user who started the import.</summary>
    public string? StartedBy { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}

/// <summary>Paged error query (an import can reject thousands of rows).</summary>
public class EmployeeImportErrorListInput : PagedAndSortedResultRequestDto
{
    public int? RowNumber { get; set; }
}

/// <summary>Real-time payload pushed over SignalR. The database stays authoritative.</summary>
public class EmployeeImportNotificationDto
{
    /// <summary>"EmployeeImportProgress" | "EmployeeImportCompleted"</summary>
    public string Type { get; set; } = string.Empty;

    public Guid ImportJobId { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }

    public int TotalChunks { get; set; }
    public int CompletedChunks { get; set; }
    public int CurrentChunk { get; set; }

    public int ProgressPercentage { get; set; }

    public string? Message { get; set; }
}
