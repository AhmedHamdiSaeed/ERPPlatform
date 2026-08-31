using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace ERPPlatform.Hubs;

[Authorize]
[HubRoute("/signalr-hubs/notification")]
public class NotificationHub : AbpHub
{
    public async Task SendNotification(string message)
    {
        // Broadcast to all connected clients
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendNotificationToUser(string targetUserId, string message)
    {
        // Send to a specific user (mapped via IUserIdProvider / NameIdentifier claim)
        await Clients.User(targetUserId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendNotificationToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
