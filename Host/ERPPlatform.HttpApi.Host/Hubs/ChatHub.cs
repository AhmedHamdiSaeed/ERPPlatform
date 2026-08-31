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
        var senderUsername = CurrentUser.UserName ?? "Unknown";

        // Send to the specific target user (mapped via IUserIdProvider / NameIdentifier claim)
        if (!string.IsNullOrEmpty(targetUserId))
        {
            await Clients.User(targetUserId).SendAsync("ReceiveMessage", senderUsername, message);
        }

        // Also echo back to the sender
        await Clients.Caller.SendAsync("ReceiveMessage", senderUsername, message);
    }

    public async Task SendMessageToChannel(string channelName, string message)
    {
        var senderUsername = CurrentUser.UserName ?? "Unknown";
        await Clients.Group(channelName).SendAsync("ReceiveChannelMessage", channelName, senderUsername, message);
    }

    public async Task JoinChannel(string channelName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channelName);
    }

    public async Task LeaveChannel(string channelName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelName);
    }
}
