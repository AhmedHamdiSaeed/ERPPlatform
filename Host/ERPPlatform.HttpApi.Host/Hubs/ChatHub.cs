using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace ERPPlatform.Hubs;

[Authorize]
[HubRoute("/signalr-hubs/chat")]
public class ChatHub : AbpHub
{
    public async Task SendMessage(string targetUserId, string message)
    {
        // For simplicity, broadcast to all or a specific group.
        // In a real application, you'd send to the specific User ID connection.
        var senderUsername = CurrentUser.UserName ?? "Unknown";
        
        await Clients.All.SendAsync("ReceiveMessage", senderUsername, message);
    }
}
