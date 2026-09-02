using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Application.Chat;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform;

/// <summary>
/// SignalR implementation of <see cref="IChatNotifier"/>. Lives in the Host because that is
/// where the hub is registered, keeping the application layer free of SignalR dependencies.
/// </summary>
public class SignalRChatNotifier : IChatNotifier, ITransientDependency
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IRepository<ChatParticipant, Guid> _participantRepository;

    public SignalRChatNotifier(
        IHubContext<ChatHub> hubContext,
        IRepository<ChatParticipant, Guid> participantRepository)
    {
        _hubContext = hubContext;
        _participantRepository = participantRepository;
    }

    public async Task NotifyMessageAsync(ChatMessageDto message)
    {
        await SendToMembersAsync(message.ConversationId, "MessageReceived", message);
    }

    public async Task NotifyMessageEditedAsync(ChatMessageDto message)
    {
        await SendToMembersAsync(message.ConversationId, "MessageEdited", message);
    }

    public async Task NotifyMessageDeletedAsync(Guid conversationId, Guid messageId)
    {
        await SendToMembersAsync(conversationId, "MessageDeleted", new { conversationId, messageId });
    }

    public async Task NotifyReactionAsync(Guid conversationId, ChatMessageDto message)
    {
        await SendToMembersAsync(conversationId, "MessageReactionChanged", message);
    }

    public async Task NotifyTypingAsync(Guid conversationId, string userId, string userName, bool isTyping)
    {
        // Typing is only relevant to people looking at the thread right now.
        await _hubContext.Clients
            .Group(ChatHub.ConversationGroup(conversationId))
            .SendAsync("UserTyping", conversationId, userId, userName, isTyping);
    }

    public async Task NotifyReadAsync(Guid conversationId, string userId, DateTime readAt)
    {
        await SendToMembersAsync(conversationId, "ConversationRead", new { conversationId, userId, readAt });
    }

    public async Task NotifyParticipantsChangedAsync(Guid conversationId)
    {
        await SendToMembersAsync(conversationId, "ParticipantsChanged", new { conversationId });
    }

    /// <summary>
    /// Fans an event out to every active member of a conversation via their personal group,
    /// so a member receives it whether or not they currently have the thread open.
    /// </summary>
    private async Task SendToMembersAsync(Guid conversationId, string method, object payload)
    {
        var userIds = await GetMemberUserIdsAsync(conversationId);

        if (userIds.Count == 0) return;

        var clients = userIds.Select(id => ChatHub.UserGroup(id)).ToList();

        await _hubContext.Clients.Groups(clients).SendAsync(method, payload);
    }

    private async Task<List<string>> GetMemberUserIdsAsync(Guid conversationId)
    {
        var participants = await _participantRepository.GetListAsync(
            p => p.ConversationId == conversationId && !p.LeftAt.HasValue);

        return participants
            .Select(p => p.UserId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();
    }
}
