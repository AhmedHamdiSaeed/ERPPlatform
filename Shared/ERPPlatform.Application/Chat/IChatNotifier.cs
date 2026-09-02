using System;
using System.Threading.Tasks;

namespace ERPPlatform.Application.Chat;

/// <summary>
/// Abstraction over the real-time transport so the application layer can push chat
/// events without taking a dependency on SignalR (which lives in the Host project).
/// Implemented by <c>SignalRChatNotifier</c> in ERPPlatform.HttpApi.Host.
/// </summary>
public interface IChatNotifier
{
    Task NotifyMessageAsync(ChatMessageDto message);

    Task NotifyMessageEditedAsync(ChatMessageDto message);

    Task NotifyMessageDeletedAsync(Guid conversationId, Guid messageId);

    Task NotifyReactionAsync(Guid conversationId, ChatMessageDto message);

    /// <summary>Broadcasts that a user started/stopped typing. Never persisted.</summary>
    Task NotifyTypingAsync(Guid conversationId, string userId, string userName, bool isTyping);

    Task NotifyReadAsync(Guid conversationId, string userId, DateTime readAt);

    /// <summary>Members were added/removed, or group details changed.</summary>
    Task NotifyParticipantsChangedAsync(Guid conversationId);
}
