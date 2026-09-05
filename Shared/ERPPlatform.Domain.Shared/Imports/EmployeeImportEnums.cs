namespace ERPPlatform.Imports;

/// <summary>
/// Lifecycle status of a whole employee Excel import.
/// Persisted on <c>EmployeeImportJob.Status</c> — this (not the browser, not SignalR)
/// is the single source of truth for what happened to an import.
/// </summary>
public enum EmployeeImportStatus
{
    /// <summary>Accepted by the API; the coordinator job has not started yet.</summary>
    Queued = 0,

    /// <summary>At least one chunk is being worked on.</summary>
    Processing = 1,

    /// <summary>Every chunk finished and every row was imported.</summary>
    Completed = 2,

    /// <summary>Every chunk finished, but at least one row was rejected.</summary>
    CompletedWithErrors = 3,

    /// <summary>Processing could not continue (fatal error / chunk exhausted its retries).</summary>
    Failed = 4,

    /// <summary>An administrator cancelled the import.</summary>
    Cancelled = 5
}

/// <summary>
/// Status of a single slice of rows. Each chunk is the unit of work, of retry and
/// of resumability: a completed chunk is never re-processed.
/// </summary>
public enum EmployeeImportChunkStatus
{
    /// <summary>Not started yet (or released after a failed attempt, eligible for retry).</summary>
    Pending = 0,

    /// <summary>Claimed by a worker and in-flight.</summary>
    Processing = 1,

    /// <summary>Finished. Skipped on any subsequent run.</summary>
    Completed = 2,

    /// <summary>Attempt failed; the chunk is released back to <see cref="Pending"/> for retry.</summary>
    Failed = 3,

    /// <summary>Skipped because the parent job was cancelled.</summary>
    Cancelled = 4
}
