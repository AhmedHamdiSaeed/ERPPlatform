using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace ERPPlatform.Application.Chat;

public class ChatParticipantDto : EntityDto<Guid>
{
    public Guid ConversationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public bool IsMuted { get; set; }
}

public class ChatConversationDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public ChatConversationType Type { get; set; }
    public string CreatorUserId { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string LastMessageSenderName { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = string.Empty;
    public bool IsArchived { get; set; }

    public int MemberCount { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public bool IsAdmin { get; set; }

    public List<ChatParticipantDto> Participants { get; set; } = new();

    /// <summary>For direct chats: the person on the other side (used for title/avatar).</summary>
    public ChatParticipantDto? OtherParticipant { get; set; }
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;

    /// <summary>User ids to add. The creator is added automatically and does not need to be listed.</summary>
    public List<string> MemberUserIds { get; set; } = new();

    public string Description { get; set; } = string.Empty;
}

public class UpdateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class AddMembersDto
{
    public List<string> MemberUserIds { get; set; } = new();
}

public class DirectChatDto
{
    public string OtherUserId { get; set; } = string.Empty;
}

public class UserLookupDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Authorize]
public class ChatConversationAppService : ApplicationService
{
    private readonly IRepository<ChatConversation, Guid> _conversationRepository;
    private readonly IRepository<ChatParticipant, Guid> _participantRepository;
    private readonly IRepository<ChatMessage, Guid> _messageRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IChatNotifier _notifier;

    public ChatConversationAppService(
        IRepository<ChatConversation, Guid> conversationRepository,
        IRepository<ChatParticipant, Guid> participantRepository,
        IRepository<ChatMessage, Guid> messageRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IChatNotifier notifier)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _notifier = notifier;
    }

    /// <summary>Every conversation the caller belongs to, most recently active first.</summary>
    public virtual async Task<ListResultDto<ChatConversationDto>> GetMyConversationsAsync()
    {
        var userId = GetCurrentUserId();

        var memberships = await _participantRepository.GetListAsync(p => p.UserId == userId && !p.LeftAt.HasValue);
        var conversationIds = memberships.Select(m => m.ConversationId).ToHashSet();

        if (conversationIds.Count == 0)
        {
            return new ListResultDto<ChatConversationDto>(new List<ChatConversationDto>());
        }

        var conversations = (await _conversationRepository.GetListAsync())
            .Where(c => conversationIds.Contains(c.Id))
            .ToList();

        var allParticipants = (await _participantRepository.GetListAsync())
            .Where(p => conversationIds.Contains(p.ConversationId) && !p.LeftAt.HasValue)
            .ToList();

        var messages = (await _messageRepository.GetListAsync())
            .Where(m => m.ConversationId.HasValue && conversationIds.Contains(m.ConversationId.Value))
            .ToList();

        var users = await _userRepository.GetListAsync();
        var userLookup = users.ToDictionary(u => u.Id.ToString(), u => u);

        var result = new List<ChatConversationDto>();

        foreach (var conversation in conversations)
        {
            var membership = memberships.First(m => m.ConversationId == conversation.Id);
            var participants = allParticipants.Where(p => p.ConversationId == conversation.Id).ToList();

            var unread = messages.Count(m =>
                m.ConversationId == conversation.Id &&
                m.SenderId != userId &&
                !m.IsDeletedBySender &&
                (!membership.LastReadAt.HasValue || m.Timestamp > membership.LastReadAt.Value));

            var participantDtos = participants.Select(p => ToParticipantDto(p, userLookup)).ToList();

            result.Add(new ChatConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                Description = conversation.Description,
                AvatarUrl = conversation.AvatarUrl,
                Type = conversation.Type,
                CreatorUserId = conversation.CreatorUserId,
                CreationTime = conversation.CreationTime,
                LastMessageAt = conversation.LastMessageAt,
                LastMessageSenderName = conversation.LastMessageSenderName,
                LastMessagePreview = conversation.LastMessagePreview,
                IsArchived = conversation.IsArchived,
                MemberCount = participants.Count,
                UnreadCount = unread,
                IsMuted = membership.IsMuted,
                IsAdmin = membership.IsAdmin,
                Participants = participantDtos,
                OtherParticipant = conversation.Type == ChatConversationType.Direct
                    ? participantDtos.FirstOrDefault(p => p.UserId != userId)
                    : null
            });
        }

        return new ListResultDto<ChatConversationDto>(
            result.OrderByDescending(c => c.LastMessageAt ?? c.CreationTime).ToList());
    }

    /// <summary>Creates a group with the given members. The creator is added as an admin automatically.</summary>
    public virtual async Task<ChatConversationDto> CreateGroupAsync(CreateGroupDto input)
    {
        var userId = GetCurrentUserId();

        var name = (input.Name ?? string.Empty).Trim();
        if (name.Length == 0)
        {
            throw new UserFriendlyException("Group name is required.");
        }

        var memberIds = (input.MemberUserIds ?? new List<string>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        // The creator is always a member (and admin) - no need to pass their own id.
        memberIds.Remove(userId);

        var conversation = new ChatConversation
        {
            Name = name,
            Description = input.Description ?? string.Empty,
            Type = ChatConversationType.Group,
            CreatorUserId = userId
        };

        await _conversationRepository.InsertAsync(conversation, autoSave: true);

        var participants = new List<ChatParticipant>
        {
            new()
            {
                ConversationId = conversation.Id,
                UserId = userId,
                UserName = CurrentUser.UserName ?? string.Empty,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            }
        };

        foreach (var memberId in memberIds)
        {
            var user = await _userRepository.FindAsync(Guid.Parse(memberId));
            if (user == null) continue;

            participants.Add(new ChatParticipant
            {
                ConversationId = conversation.Id,
                UserId = memberId,
                UserName = user.UserName,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _participantRepository.InsertManyAsync(participants, autoSave: true);

        return await BuildConversationDtoAsync(conversation.Id);
    }

    public virtual async Task<ChatConversationDto> UpdateGroupAsync(Guid id, UpdateGroupDto input)
    {
        await EnsureAdminAsync(id);

        var conversation = await _conversationRepository.GetAsync(id);

        var name = (input.Name ?? string.Empty).Trim();
        if (name.Length == 0)
        {
            throw new UserFriendlyException("Group name is required.");
        }

        conversation.Name = name;
        conversation.Description = input.Description ?? string.Empty;
        conversation.AvatarUrl = input.AvatarUrl ?? string.Empty;

        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
        await _notifier.NotifyParticipantsChangedAsync(id);

        return await BuildConversationDtoAsync(id);
    }

    /// <summary>Adds one or more users to an existing group.</summary>
    public virtual async Task<ChatConversationDto> AddMembersAsync(Guid id, AddMembersDto input)
    {
        await EnsureAdminAsync(id);

        var memberIds = (input.MemberUserIds ?? new List<string>())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
        {
            throw new UserFriendlyException("Select at least one user to add.");
        }

        var existing = await _participantRepository.GetListAsync(p => p.ConversationId == id);
        var existingIds = existing.Select(p => p.UserId).ToHashSet();

        var toAdd = new List<ChatParticipant>();

        foreach (var memberId in memberIds)
        {
            if (existingIds.Contains(memberId)) continue;

            var user = await _userRepository.FindAsync(Guid.Parse(memberId));
            if (user == null) continue;

            toAdd.Add(new ChatParticipant
            {
                ConversationId = id,
                UserId = memberId,
                UserName = user.UserName,
                JoinedAt = DateTime.UtcNow
            });
            existingIds.Add(memberId);
        }

        if (toAdd.Count > 0)
        {
            await _participantRepository.InsertManyAsync(toAdd, autoSave: true);
            await _notifier.NotifyParticipantsChangedAsync(id);
        }

        return await BuildConversationDtoAsync(id);
    }

    public virtual async Task RemoveMemberAsync(Guid id, string userId)
    {
        await EnsureAdminAsync(id);

        var participant = (await _participantRepository.GetListAsync(
                p => p.ConversationId == id && p.UserId == userId && !p.LeftAt.HasValue))
            .FirstOrDefault();

        if (participant == null) return;

        participant.LeftAt = DateTime.UtcNow;
        await _participantRepository.UpdateAsync(participant, autoSave: true);
        await _notifier.NotifyParticipantsChangedAsync(id);
    }

    public virtual async Task LeaveAsync(Guid id)
    {
        var userId = GetCurrentUserId();

        var participant = (await _participantRepository.GetListAsync(
                p => p.ConversationId == id && p.UserId == userId && !p.LeftAt.HasValue))
            .FirstOrDefault();

        if (participant == null)
        {
            throw new UserFriendlyException("You are not a member of this conversation.");
        }

        participant.LeftAt = DateTime.UtcNow;
        await _participantRepository.UpdateAsync(participant, autoSave: true);
        await _notifier.NotifyParticipantsChangedAsync(id);
    }

    public virtual async Task<ListResultDto<ChatParticipantDto>> GetParticipantsAsync(Guid id)
    {
        await EnsureMemberAsync(id);

        var participants = (await _participantRepository.GetListAsync(
                p => p.ConversationId == id && !p.LeftAt.HasValue))
            .ToList();

        var users = await _userRepository.GetListAsync();
        var userLookup = users.ToDictionary(u => u.Id.ToString(), u => u);

        return new ListResultDto<ChatParticipantDto>(
            participants.Select(p => ToParticipantDto(p, userLookup)).ToList());
    }

    /// <summary>
    /// Returns the existing 1:1 thread with a user, creating it if needed. A deterministic
    /// key prevents duplicate direct threads between the same two people.
    /// </summary>
    // NOTE: named "StartDirect" rather than "GetOrCreateDirect" - ABP infers the HTTP verb
    // from the method name, and a "Get..." prefix would expose this as a GET, which is wrong
    // for a call that can create a row.
    public virtual async Task<ChatConversationDto> StartDirectAsync(string otherUserId)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(otherUserId))
        {
            throw new UserFriendlyException("A target user is required.");
        }

        if (otherUserId == userId)
        {
            throw new UserFriendlyException("You cannot start a direct chat with yourself.");
        }

        var other = await _userRepository.FindAsync(Guid.Parse(otherUserId));
        if (other == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        var ids = new[] { userId, otherUserId }.OrderBy(x => x).ToArray();
        var key = $"dm:{ids[0]}:{ids[1]}";

        var existing = (await _conversationRepository.GetListAsync(
                c => c.Type == ChatConversationType.Direct && c.Name == key))
            .FirstOrDefault();

        if (existing != null)
        {
            return await BuildConversationDtoAsync(existing.Id);
        }

        var conversation = new ChatConversation
        {
            Name = key,
            Type = ChatConversationType.Direct,
            CreatorUserId = userId
        };

        await _conversationRepository.InsertAsync(conversation, autoSave: true);

        await _participantRepository.InsertManyAsync(new[]
        {
            new ChatParticipant
            {
                ConversationId = conversation.Id,
                UserId = userId,
                UserName = CurrentUser.UserName ?? string.Empty,
                JoinedAt = DateTime.UtcNow
            },
            new ChatParticipant
            {
                ConversationId = conversation.Id,
                UserId = otherUserId,
                UserName = other.UserName,
                JoinedAt = DateTime.UtcNow
            }
        }, autoSave: true);

        return await BuildConversationDtoAsync(conversation.Id);
    }

    /// <summary>User picker for the "add members" dialog. Excludes the caller and existing members when id is given.</summary>
    public virtual async Task<ListResultDto<UserLookupDto>> GetAvailableUsersAsync(Guid? conversationId, string? filter = "", int maxResultCount = 20)
    {
        var userId = GetCurrentUserId();
        var term = (filter ?? string.Empty).Trim().ToLowerInvariant();

        if (maxResultCount <= 0 || maxResultCount > 100) maxResultCount = 20;

        var users = await _userRepository.GetListAsync();

        var exclude = new HashSet<string> { userId };

        if (conversationId.HasValue)
        {
            var participants = await _participantRepository.GetListAsync(
                p => p.ConversationId == conversationId.Value && !p.LeftAt.HasValue);

            foreach (var participant in participants)
            {
                exclude.Add(participant.UserId);
            }
        }

        var result = users
            .Where(u => !exclude.Contains(u.Id.ToString()))
            .Where(u => term.Length == 0 ||
                        u.UserName.ToLowerInvariant().Contains(term) ||
                        (u.Email ?? string.Empty).ToLowerInvariant().Contains(term) ||
                        (u.Name ?? string.Empty).ToLowerInvariant().Contains(term) ||
                        (u.Surname ?? string.Empty).ToLowerInvariant().Contains(term))
            .OrderBy(u => u.UserName)
            .Take(maxResultCount)
            .Select(u => new UserLookupDto
            {
                Id = u.Id.ToString(),
                UserName = u.UserName,
                DisplayName = string.Join(" ", new[] { u.Name, u.Surname }.Where(s => !string.IsNullOrWhiteSpace(s))),
                Email = u.Email ?? string.Empty
            })
            .ToList();

        return new ListResultDto<UserLookupDto>(result);
    }

    /// <summary>
    /// User picker for the "create new group" dialog. There is no conversation yet, so we
    /// can't scope by membership - this simply returns every user except the caller. Exposed
    /// at a separate route because <see cref="GetAvailableUsersAsync"/> requires a conversation
    /// id in the path, which doesn't exist during group creation.
    /// </summary>
    public virtual async Task<ListResultDto<UserLookupDto>> GetAvailableUsersForNewGroupAsync(string? filter = "", int maxResultCount = 20)
    {
        var userId = GetCurrentUserId();
        var term = (filter ?? string.Empty).Trim().ToLowerInvariant();

        if (maxResultCount <= 0 || maxResultCount > 100) maxResultCount = 20;

        var users = await _userRepository.GetListAsync();

        var result = users
            .Where(u => u.Id.ToString() != userId)
            .Where(u => term.Length == 0 ||
                        u.UserName.ToLowerInvariant().Contains(term) ||
                        (u.Email ?? string.Empty).ToLowerInvariant().Contains(term) ||
                        (u.Name ?? string.Empty).ToLowerInvariant().Contains(term) ||
                        (u.Surname ?? string.Empty).ToLowerInvariant().Contains(term))
            .OrderBy(u => u.UserName)
            .Take(maxResultCount)
            .Select(u => new UserLookupDto
            {
                Id = u.Id.ToString(),
                UserName = u.UserName,
                DisplayName = string.Join(" ", new[] { u.Name, u.Surname }.Where(s => !string.IsNullOrWhiteSpace(s))),
                Email = u.Email ?? string.Empty
            })
            .ToList();

        return new ListResultDto<UserLookupDto>(result);
    }

    public virtual async Task MuteAsync(Guid id, bool isMuted)
    {
        var userId = GetCurrentUserId();
        var participant = await GetMembershipAsync(id, userId);

        participant.IsMuted = isMuted;
        await _participantRepository.UpdateAsync(participant, autoSave: true);
    }

    // ---------- helpers ----------

    private string GetCurrentUserId()
    {
        return CurrentUser.Id?.ToString() ?? throw new UserFriendlyException("User is not authenticated.");
    }

    private async Task<ChatParticipant> GetMembershipAsync(Guid conversationId, string userId)
    {
        var participant = (await _participantRepository.GetListAsync(
                p => p.ConversationId == conversationId && p.UserId == userId && !p.LeftAt.HasValue))
            .FirstOrDefault();

        return participant ?? throw new UserFriendlyException("You are not a member of this conversation.");
    }

    private async Task EnsureMemberAsync(Guid conversationId)
    {
        await GetMembershipAsync(conversationId, GetCurrentUserId());
    }

    private async Task EnsureAdminAsync(Guid conversationId)
    {
        var participant = await GetMembershipAsync(conversationId, GetCurrentUserId());

        if (!participant.IsAdmin)
        {
            throw new UserFriendlyException("Only group admins can perform this action.");
        }
    }

    private static ChatParticipantDto ToParticipantDto(ChatParticipant p, Dictionary<string, IdentityUser> userLookup)
    {
        userLookup.TryGetValue(p.UserId, out var user);

        var displayName = user == null
            ? p.UserName
            : string.Join(" ", new[] { user.Name, user.Surname }.Where(s => !string.IsNullOrWhiteSpace(s)));

        return new ChatParticipantDto
        {
            Id = p.Id,
            ConversationId = p.ConversationId,
            UserId = p.UserId,
            UserName = p.UserName,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? p.UserName : displayName,
            Email = user?.Email ?? string.Empty,
            IsAdmin = p.IsAdmin,
            JoinedAt = p.JoinedAt,
            LastReadAt = p.LastReadAt,
            IsMuted = p.IsMuted
        };
    }

    private async Task<ChatConversationDto> BuildConversationDtoAsync(Guid conversationId)
    {
        var userId = GetCurrentUserId();

        var conversation = await _conversationRepository.GetAsync(conversationId);
        var membership = await GetMembershipAsync(conversationId, userId);

        var participants = await _participantRepository.GetListAsync(
            p => p.ConversationId == conversationId && !p.LeftAt.HasValue);

        var messages = await _messageRepository.GetListAsync(m => m.ConversationId == conversationId);

        var unread = messages.Count(m =>
            m.SenderId != userId &&
            !m.IsDeletedBySender &&
            (!membership.LastReadAt.HasValue || m.Timestamp > membership.LastReadAt.Value));

        var users = await _userRepository.GetListAsync();
        var userLookup = users.ToDictionary(u => u.Id.ToString(), u => u);
        var participantDtos = participants.Select(p => ToParticipantDto(p, userLookup)).ToList();

        return new ChatConversationDto
        {
            Id = conversation.Id,
            Name = conversation.Name,
            Description = conversation.Description,
            AvatarUrl = conversation.AvatarUrl,
            Type = conversation.Type,
            CreatorUserId = conversation.CreatorUserId,
            CreationTime = conversation.CreationTime,
            LastMessageAt = conversation.LastMessageAt,
            LastMessageSenderName = conversation.LastMessageSenderName,
            LastMessagePreview = conversation.LastMessagePreview,
            IsArchived = conversation.IsArchived,
            MemberCount = participants.Count,
            UnreadCount = unread,
            IsMuted = membership.IsMuted,
            IsAdmin = membership.IsAdmin,
            Participants = participantDtos,
            OtherParticipant = conversation.Type == ChatConversationType.Direct
                ? participantDtos.FirstOrDefault(p => p.UserId != userId)
                : null
        };
    }
}
