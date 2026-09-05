using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Imports;

/// <summary>
/// SignalR implementation of the import notifier. Pushes progress and completion
/// events to the single user who started the import, over the notification hub.
///
/// Delivery is best-effort: the application layer swallows any transport failure so a
/// broken hub never affects the import itself. The database stays authoritative and
/// the client recovers by polling the status endpoint.
/// </summary>
public class EmployeeImportSignalRNotifier : IEmployeeImportNotifier, ITransientDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public EmployeeImportSignalRNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProgressAsync(string userId, EmployeeImportNotificationDto payload)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        await _hubContext.Clients.User(userId)
            .SendAsync(EmployeeImportNotificationTypes.Progress, payload);
    }

    public async Task NotifyCompletedAsync(string userId, EmployeeImportNotificationDto payload)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        await _hubContext.Clients.User(userId)
            .SendAsync(EmployeeImportNotificationTypes.Completed, payload);
    }
}
