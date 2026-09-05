using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using Hangfire;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Imports;

/// <summary>
/// Hangfire implementation of the import scheduler. Each chunk is enqueued as an
/// independent background job on the dedicated <c>employee-import</c> queue (see the
/// <see cref="QueueAttribute"/> on <see cref="EmployeeImportChunkHangfireJob"/>).
///
/// Because the resume logic lives in the chunk processor — an atomic claim plus
/// recomputed counters — retrying a failed chunk never restarts the whole file.
/// </summary>
public class EmployeeImportHangfireScheduler : IEmployeeImportJobScheduler, ITransientDependency
{
    private readonly IBackgroundJobClient _jobClient;

    public EmployeeImportHangfireScheduler(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public Task EnqueueCoordinatorAsync(EmployeeImportScheduleArgs args)
    {
        // The orchestrator already persists the chunk rows synchronously and then asks
        // us to enqueue the first chunk, so the "coordinator" simply kicks off chunk 1.
        // Kept as an explicit method for clarity and future fan-out.
        _jobClient.Enqueue<EmployeeImportChunkHangfireJob>(j => j.ProcessAsync(args.ImportJobId, 1, args));
        return Task.CompletedTask;
    }

    public Task EnqueueChunkAsync(EmployeeImportScheduleArgs args, int chunkNumber)
    {
        _jobClient.Enqueue<EmployeeImportChunkHangfireJob>(j => j.ProcessAsync(args.ImportJobId, chunkNumber, args));
        return Task.CompletedTask;
    }
}
