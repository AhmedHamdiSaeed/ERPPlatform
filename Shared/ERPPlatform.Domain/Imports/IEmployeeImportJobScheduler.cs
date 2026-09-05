using System.Threading.Tasks;

namespace ERPPlatform.Domain.Imports;

/// <summary>
/// Abstraction over the background-job engine so the application layer can schedule
/// import work without depending on Hangfire (which is wired up in the Host project).
/// </summary>
public interface IEmployeeImportJobScheduler
{
    /// <summary>
    /// Queues the coordinator: it reads the spreadsheet, creates the chunk rows and
    /// kicks off the first chunk. Safe to call more than once — the coordinator
    /// refuses to re-chunk a job that already has chunks.
    /// </summary>
    Task EnqueueCoordinatorAsync(EmployeeImportScheduleArgs args);

    /// <summary>
    /// Queues a single chunk. Hangfire's <c>[AutomaticRetry]</c> re-runs the chunk
    /// in place, so a failure never restarts the file from row 1.
    /// </summary>
    Task EnqueueChunkAsync(EmployeeImportScheduleArgs args, int chunkNumber);
}

public class EmployeeImportScheduleArgs
{
    public Guid ImportJobId { get; set; }

    public Guid? TenantId { get; set; }

    /// <summary>User who started the import; re-established inside the worker for auditing and notifications.</summary>
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    /// <summary>Avoids two scheduler calls for the same chunk creating duplicate Hangfire jobs.</summary>
    public string? QueueKey { get; set; }
}
