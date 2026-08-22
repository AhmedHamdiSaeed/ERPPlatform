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
        // Broadcast the message to all clients
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
