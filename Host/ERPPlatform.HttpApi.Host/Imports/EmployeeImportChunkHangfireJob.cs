using System;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using Hangfire;
using Volo.Abp.MultiTenancy;

namespace ERPPlatform.Imports;

/// <summary>
/// The Hangfire-fired unit of work: exactly one chunk.
///
/// It delegates to the application-layer <see cref="EmployeeImportChunkProcessor"/>,
/// which owns the resume guarantee (atomic claim + recomputed counters + idempotent
/// upserts). When the processor reports a next chunk, this job enqueues it, forming a
/// chain of independently-retryable chunks.
///
/// The whole run is wrapped in the import's tenant via <see cref="ICurrentTenant.Change"/>
/// so EF's tenant global filter sees the right rows — background jobs have no HTTP
/// request to establish the tenant.
/// </summary>
[Queue("employee-import")]
public class EmployeeImportChunkHangfireJob
{
    private readonly EmployeeImportChunkProcessor _processor;
    private readonly IEmployeeImportJobScheduler _scheduler;
    private readonly ICurrentTenant _currentTenant;

    public EmployeeImportChunkHangfireJob(
        EmployeeImportChunkProcessor processor,
        IEmployeeImportJobScheduler scheduler,
        ICurrentTenant currentTenant)
    {
        _processor = processor;
        _scheduler = scheduler;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// Mirrors <c>EmployeeImportOptions.MaxChunkAttempts</c>. A thrown
    /// <see cref="EmployeeImportChunkException"/> triggers a Hangfire retry of THIS
    /// chunk only; the chunk processor's own budget finalises the job as Failed once
    /// attempts are exhausted.
    /// </summary>
    [AutomaticRetry(Attempts = 5)]
    public async Task ProcessAsync(Guid importJobId, int chunkNumber, EmployeeImportScheduleArgs args)
    {
        using (_currentTenant.Change(args.TenantId))
        {
            var outcome = await _processor.ProcessAsync(importJobId, chunkNumber);

            if (outcome.NextChunkNumber.HasValue)
            {
                await _scheduler.EnqueueChunkAsync(args, outcome.NextChunkNumber.Value);
            }
        }
    }
}
