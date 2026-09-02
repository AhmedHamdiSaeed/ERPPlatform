using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace ERPPlatform.Hubs;

[Authorize]
[HubRoute("/signalr-hubs/chat")]
public class ChatHub : AbpHub
{
    /// <summary>Every connection of a user joins this, so they receive events for
    /// conversations they are not currently viewing (drives unread badges).</summary>
    public static string UserGroup(string userId) => $"u:{userId}";

    /// <summary>Scoped to users who currently have the conversation open (typing indicators).</summary>
    public static string ConversationGroup(Guid conversationId) => $"c:{conversationId}";

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUser.Id?.ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }

    /// <summary>Client calls this when it opens a conversation, to receive typing indicators.</summary>
    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    /// <summary>Ephemeral typing signal - broadcast to others in the conversation, never persisted.</summary>
    public async Task SetTyping(Guid conversationId, bool isTyping)
    {
        var userId = CurrentUser.Id?.ToString();
        if (string.IsNullOrEmpty(userId)) return;

        var userName = CurrentUser.Name ?? CurrentUser.UserName ?? "Someone";

        await Clients.OthersInGroup(ConversationGroup(conversationId))
            .SendAsync("UserTyping", conversationId, userId, userName, isTyping);
    }

    // ---- legacy broadcast API (kept for backwards compatibility) ----

    public async Task SendMessage(string targetUserId, string message)
    {
        var senderUsername = CurrentUser.UserName ?? "Unknown";

        if (!string.IsNullOrEmpty(targetUserId))
        {
            await Clients.User(targetUserId).SendAsync("ReceiveMessage", senderUsername, message);
        }

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
