namespace ERPPlatform.Application.Imports;

/// <summary>
/// Bound from the <c>EmployeeImport</c> section of appsettings.json, so the chunk
/// size and the safety caps are tunable per environment without a redeploy.
/// </summary>
public class EmployeeImportOptions
{
    /// <summary>Rows processed per Hangfire job. 10,000 rows / 100 = 100 chunks.</summary>
    public int ChunkSize { get; set; } = EmployeeImportConsts.DefaultChunkSize;

    public long MaxFileSizeBytes { get; set; } = EmployeeImportConsts.MaxFileSizeBytes;

    public int MaxTotalRows { get; set; } = EmployeeImportConsts.MaxTotalRows;

    /// <summary>
    /// How many times a single chunk may be attempted before the whole job is
    /// marked <c>Failed</c>. Hangfire retries the chunk in place; each retry is
    /// recorded in <c>EmployeeImportChunk.AttemptCount</c>.
    /// </summary>
    public int MaxChunkAttempts { get; set; } = EmployeeImportConsts.MaxChunkAttempts;

    /// <summary>
    /// Push a SignalR progress message only every N completed chunks. Keeps a
    /// 100-chunk import from flooding the hub. The database is updated every chunk.
    /// </summary>
    public int ProgressNotificationEveryNChunks { get; set; } = 1;

    /// <summary>
    /// Days the retained source spreadsheet is kept after the import reaches a
    /// terminal state. History rows in the database are never auto-deleted.
    /// </summary>
    public int SourceFileRetentionDays { get; set; } = 30;

    /// <summary>Hangfire queue used for import work.</summary>
    public string QueueName { get; set; } = "employee-import";

    /// <summary>
    /// A chunk left in <c>Processing</c> longer than this (minutes) is assumed dead
    /// and released by the recovery watchdog. Must be larger than Hangfire's own
    /// acknowledgement timeout so a merely slow chunk is not double-run.
    /// </summary>
    public int StuckChunkTimeoutMinutes { get; set; } = 30;

    /// <summary>Set false to keep the source file even after a clean import.</summary>
    public bool DeleteSourceFileAfterSuccess { get; set; } = true;
}
