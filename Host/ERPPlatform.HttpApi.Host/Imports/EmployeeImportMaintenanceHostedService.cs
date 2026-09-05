using System;
using System.Threading;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Imports;

/// <summary>
/// Periodic housekeeping for the import pipeline:
///  <list type="bullet">
///    <item>Releases chunks left <c>Processing</c> after a worker crash / restart and
///          re-queues the first incomplete chunk of every still-active job — the
///          server-side half of the resume guarantee.</item>
///    <item>Deletes retained source spreadsheets once the retention window has passed.</item>
///  </list>
/// Runs with the host/default tenant; tenant-scoped recovery is also covered by the
/// user-initiated Retry, which runs inside the caller's tenant context.
/// </summary>
public class EmployeeImportMaintenanceHostedService : BackgroundService, ISingletonDependency
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmployeeImportOptions _options;
    private readonly ILogger<EmployeeImportMaintenanceHostedService> _logger;

    public EmployeeImportMaintenanceHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<EmployeeImportOptions> options,
        ILogger<EmployeeImportMaintenanceHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the application finish booting before the first sweep.
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var orchestrator = scope.ServiceProvider.GetRequiredService<EmployeeImportOrchestrator>();
                    var argsFactory = scope.ServiceProvider.GetRequiredService<IEmployeeImportScheduleArgsFactory>();

                    var recovered = await orchestrator.RecoverStalledJobsAsync(argsFactory);
                    if (recovered > 0)
                    {
                        _logger.LogInformation("Employee import recovery released {Count} stalled job(s).", recovered);
                    }

                    var deleted = await orchestrator.CleanupExpiredSourceFilesAsync();
                    if (deleted > 0)
                    {
                        _logger.LogInformation("Employee import cleanup removed {Count} expired source file(s).", deleted);
                    }
                }
            }
            catch (Exception ex)
            {
                // A failing sweep must never take the host down; just retry next interval.
                _logger.LogWarning(ex, "Employee import maintenance sweep failed; retrying next interval.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
