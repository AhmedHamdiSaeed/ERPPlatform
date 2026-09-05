using System;
using ERPPlatform.Imports;
using Volo.Abp.Domain.Entities;

namespace ERPPlatform.Domain.Imports;

/// <summary>
/// A horizontal slice of the source spreadsheet. Chunks are the resume unit: after a
/// crash, chunks already marked <see cref="EmployeeImportChunkStatus.Completed"/> are
/// skipped and processing restarts at the first incomplete one.
/// </summary>
public class EmployeeImportChunk : Entity<Guid>
{
    public Guid ImportJobId { get; set; }

    /// <summary>1-based chunk number.</summary>
    public int ChunkNumber { get; set; }

    /// <summary>1-based, inclusive. Data rows only — the header row is never part of a chunk.</summary>
    public int StartRow { get; set; }

    /// <summary>1-based, inclusive.</summary>
    public int EndRow { get; set; }

    public EmployeeImportChunkStatus Status { get; set; } = EmployeeImportChunkStatus.Pending;

    public int AttemptCount { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public string? LastError { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>Hangfire job id of the current/last invocation, useful when debugging a stuck chunk.</summary>
    public string? HangfireJobId { get; set; }

    public int RowCount => EndRow - StartRow + 1;
}
