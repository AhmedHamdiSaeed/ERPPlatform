using System;
using Volo.Abp.Domain.Entities;

namespace ERPPlatform.Domain.Imports;

/// <summary>
/// Row-level rejection. A bad row must not abort the whole import: the chunk keeps
/// going, records the problem here, and the job ends as CompletedWithErrors.
/// </summary>
public class EmployeeImportError : Entity<Guid>
{
    public Guid ImportJobId { get; set; }

    public Guid? ChunkId { get; set; }

    /// <summary>1-based spreadsheet row number, header included, so it matches what the user sees in Excel.</summary>
    public int RowNumber { get; set; }

    public string? ColumnName { get; set; }

    /// <summary>Offending value, truncated before it is stored.</summary>
    public string? Value { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
