using System;
using System.Threading.Tasks;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Real-time transport abstraction so the application layer can push import events
/// without referencing SignalR (which lives in the Host, next to the hub).
/// SignalR is a *notification only*: if it is down the import keeps running and the
/// client recovers from <c>GET /api/app/employee-import/{id}/status</c>.
/// </summary>
public interface IEmployeeImportNotifier
{
    /// <summary>Fired while the import is still running.</summary>
    Task NotifyProgressAsync(string userId, EmployeeImportNotificationDto payload);

    /// <summary>Fired once, when the import reaches a terminal state.</summary>
    Task NotifyCompletedAsync(string userId, EmployeeImportNotificationDto payload);
}
