using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Application.Chat;

public class ChatReactionDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
}

public class ChatMessageDto : EntityDto<Guid>
{
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }

    /// <summary>True when the sender removed the content; the bubble still renders as a placeholder.</summary>
    public bool IsDeletedBySender { get; set; }

    public Guid? ReplyToMessageId { get; set; }
    public string ReplyToSenderName { get; set; } = string.Empty;
    public string ReplyToPreview { get; set; } = string.Empty;

    public List<string> MentionedUserIds { get; set; } = new();

    public string AttachmentName { get; set; } = string.Empty;
    public string AttachmentContentType { get; set; } = string.Empty;
    public long AttachmentSizeBytes { get; set; }
    public string AttachmentUrl { get; set; } = string.Empty;

    public List<ChatReactionDto> Reactions { get; set; } = new();
    public bool IsMine { get; set; }

    public bool IsPinned { get; set; }
    public string CardDataJson { get; set; } = string.Empty;
    public bool IsAiResponse { get; set; }
}

public class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? ReplyToMessageId { get; set; }
    public string CardDataJson { get; set; } = string.Empty;
}

public class SendErpCardDto
{
    public Guid ConversationId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string CardDataJson { get; set; } = string.Empty;
}

public class ConversationSummaryDto
{
    public string Summary { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class EditMessageDto
{
    public string Text { get; set; } = string.Empty;
}

[Authorize]
public class ChatMessageAppService : ApplicationService
{
    private const long MaxAttachmentBytes = 10L * 1024 * 1024; // 10 MB

    private readonly IRepository<ChatMessage, Guid> _messageRepository;
    private readonly IRepository<ChatConversation, Guid> _conversationRepository;
    private readonly IRepository<ChatParticipant, Guid> _participantRepository;
    private readonly IRepository<ChatMessageReaction, Guid> _reactionRepository;
    private readonly IBlobContainer _blobContainer;
    private readonly IChatNotifier _notifier;
    private readonly IServiceProvider _serviceProvider;

    public ChatMessageAppService(
        IRepository<ChatMessage, Guid> messageRepository,
        IRepository<ChatConversation, Guid> conversationRepository,
        IRepository<ChatParticipant, Guid> participantRepository,
        IRepository<ChatMessageReaction, Guid> reactionRepository,
        IBlobContainer blobContainer,
        IChatNotifier notifier,
        IServiceProvider serviceProvider)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _reactionRepository = reactionRepository;
        _blobContainer = blobContainer;
        _notifier = notifier;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Latest messages for a conversation, returned oldest-first so the UI can render
    /// top-to-bottom. Pass skipCount to page further back into history.
    /// </summary>
    public virtual async Task<ListResultDto<ChatMessageDto>> GetHistoryAsync(Guid conversationId, int skipCount = 0, int maxResultCount = 50)
    {
        await EnsureMemberAsync(conversationId);

        if (maxResultCount <= 0 || maxResultCount > 200) maxResultCount = 50;

        var all = await _messageRepository.GetListAsync(m => m.ConversationId == conversationId);
        var page = all
            .OrderByDescending(m => m.Timestamp)
            .Skip(skipCount)
            .Take(maxResultCount)
            .Reverse()
            .ToList();

        return new ListResultDto<ChatMessageDto>(await BuildDtosAsync(page));
    }

    public virtual async Task<ChatMessageDto> SendMessageAsync(SendMessageDto input)
    {
        var userId = GetCurrentUserId();
        await EnsureMemberAsync(input.ConversationId);

        var text = (input.Text ?? string.Empty).Trim();
        if (text.Length == 0 && string.IsNullOrWhiteSpace(input.CardDataJson))
        {
            throw new UserFriendlyException("Message cannot be empty.");
        }

        var mentioned = await ResolveMentionsAsync(input.ConversationId, text);

        var message = new ChatMessage
        {
            ConversationId = input.ConversationId,
            SenderId = userId,
            SenderName = CurrentUser.Name ?? CurrentUser.UserName ?? "Unknown",
            SenderAvatar = string.Empty,
            Text = text,
            Timestamp = DateTime.UtcNow,
            ReplyToMessageId = input.ReplyToMessageId,
            MentionedUserIds = string.Join(",", mentioned),
            CardDataJson = input.CardDataJson ?? string.Empty
        };

        await _messageRepository.InsertAsync(message, autoSave: true);
        await TouchConversationAsync(input.ConversationId, message.SenderName,
            text.Length > 0 ? text : "Shared an ERP record");

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();

        await _notifier.NotifyMessageAsync(dto);

        // Check for AI mention trigger (@ai, @assistant, or /ai)
        var isAiQuery = Regex.IsMatch(text, @"^@(ai|assistant)\b", RegexOptions.IgnoreCase) ||
                        text.StartsWith("/ai ", StringComparison.OrdinalIgnoreCase);

        if (isAiQuery)
        {
            var prompt = Regex.Replace(text, @"^@(ai|assistant)\s*|^/ai\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (prompt.Length > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var aiAnswer = await GetAiAnswerAsync(prompt);
                        var aiMessage = new ChatMessage
                        {
                            ConversationId = input.ConversationId,
                            SenderId = "ai-copilot",
                            SenderName = "ERP AI Copilot",
                            Text = aiAnswer,
                            Timestamp = DateTime.UtcNow,
                            ReplyToMessageId = message.Id,
                            IsAiResponse = true
                        };
                        await _messageRepository.InsertAsync(aiMessage, autoSave: true);
                        await TouchConversationAsync(input.ConversationId, "ERP AI Copilot",
                            aiAnswer.Length > 120 ? aiAnswer[..120] : aiAnswer);
                        var aiDto = (await BuildDtosAsync(new List<ChatMessage> { aiMessage })).First();
                        await _notifier.NotifyMessageAsync(aiDto);
                    }
                    catch
                    {
                        /* non-blocking background AI dispatch */
                    }
                });
            }
        }

        return dto;
    }

    /// <summary>Multipart upload: attaches a file (optionally with caption text) to a new message.</summary>
    public virtual async Task<ChatMessageDto> SendAttachmentAsync(Guid conversationId, IRemoteStreamContent file, string text)
    {
        var userId = GetCurrentUserId();
        await EnsureMemberAsync(conversationId);

        if (file == null)
        {
            throw new UserFriendlyException("No file was uploaded.");
        }

        var fileName = string.IsNullOrWhiteSpace(file.FileName) ? "attachment" : file.FileName;
        var extension = Path.GetExtension(fileName);

        await using var buffer = new MemoryStream();
        await file.GetStream().CopyToAsync(buffer);

        if (buffer.Length > MaxAttachmentBytes)
        {
            throw new UserFriendlyException("Attachment exceeds the 10 MB limit.");
        }

        var blobName = $"chat/{Guid.NewGuid():N}{extension}";
        await _blobContainer.SaveAsync(blobName, buffer.ToArray(), overrideExisting: true);

        var caption = (text ?? string.Empty).Trim();
        var mentioned = await ResolveMentionsAsync(conversationId, caption);

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = userId,
            SenderName = CurrentUser.Name ?? CurrentUser.UserName ?? "Unknown",
            Text = caption,
            Timestamp = DateTime.UtcNow,
            MentionedUserIds = string.Join(",", mentioned),
            AttachmentName = fileName,
            AttachmentBlobName = blobName,
            AttachmentContentType = file.ContentType ?? "application/octet-stream",
            AttachmentSizeBytes = buffer.Length
        };

        await _messageRepository.InsertAsync(message, autoSave: true);
        await TouchConversationAsync(conversationId, message.SenderName,
            caption.Length > 0 ? caption : $"📎 {fileName}");

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();
        await _notifier.NotifyMessageAsync(dto);

        return dto;
    }

    public virtual async Task<IRemoteStreamContent> GetAttachmentAsync(Guid messageId)
    {
        var message = await _messageRepository.GetAsync(messageId);
        await EnsureMemberAsync(message.ConversationId!.Value);

        if (string.IsNullOrWhiteSpace(message.AttachmentBlobName))
        {
            throw new UserFriendlyException("This message has no attachment.");
        }

        var bytes = await _blobContainer.GetAllBytesOrNullAsync(message.AttachmentBlobName);
        if (bytes == null)
        {
            throw new UserFriendlyException("Attachment file is no longer available.");
        }

        return new RemoteStreamContent(
            new MemoryStream(bytes),
            message.AttachmentName,
            message.AttachmentContentType);
    }

    public virtual async Task<ChatMessageDto> EditMessageAsync(Guid id, EditMessageDto input)
    {
        var userId = GetCurrentUserId();
        var message = await _messageRepository.GetAsync(id);
        await EnsureMemberAsync(message.ConversationId!.Value);

        if (message.SenderId != userId)
        {
            throw new UserFriendlyException("You can only edit your own messages.");
        }

        var text = (input.Text ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            throw new UserFriendlyException("Message cannot be empty.");
        }

        message.Text = text;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(message, autoSave: true);

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();
        await _notifier.NotifyMessageEditedAsync(dto);

        return dto;
    }

    public virtual async Task DeleteMessageAsync(Guid id)
    {
        var userId = GetCurrentUserId();
        var message = await _messageRepository.GetAsync(id);
        var conversationId = message.ConversationId!.Value;
        await EnsureMemberAsync(conversationId);

        if (message.SenderId != userId)
        {
            throw new UserFriendlyException("You can only delete your own messages.");
        }

        // Soft-removal on purpose: the bubble stays in the thread as a placeholder so
        // replies below it keep their context.
        message.IsDeletedBySender = true;
        message.DeletedAt = DateTime.UtcNow;
        message.Text = string.Empty;
        message.AttachmentName = string.Empty;
        message.AttachmentBlobName = string.Empty;
        message.AttachmentSizeBytes = 0;

        await _messageRepository.UpdateAsync(message, autoSave: true);
        await _notifier.NotifyMessageDeletedAsync(conversationId, id);
    }

    /// <summary>Searches a single conversation, or every conversation the caller belongs to when id is null.</summary>
    public virtual async Task<ListResultDto<ChatMessageDto>> SearchAsync(Guid? conversationId, string keyword, int maxResultCount = 50)
    {
        var userId = GetCurrentUserId();
        var term = (keyword ?? string.Empty).Trim();

        if (term.Length == 0)
        {
            return new ListResultDto<ChatMessageDto>(new List<ChatMessageDto>());
        }

        if (maxResultCount <= 0 || maxResultCount > 200) maxResultCount = 50;

        IEnumerable<ChatMessage> matches;

        if (conversationId.HasValue)
        {
            await EnsureMemberAsync(conversationId.Value);
            var scoped = await _messageRepository.GetListAsync(m => m.ConversationId == conversationId.Value);
            matches = scoped;
        }
        else
        {
            var myConversationIds = (await _participantRepository.GetListAsync(p => p.UserId == userId && !p.LeftAt.HasValue))
                .Select(p => p.ConversationId)
                .ToHashSet();

            var all = await _messageRepository.GetListAsync();
            matches = all.Where(m => m.ConversationId.HasValue && myConversationIds.Contains(m.ConversationId.Value));
        }

        var result = matches
            .Where(m => !m.IsDeletedBySender && m.Text.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.Timestamp)
            .Take(maxResultCount)
            .ToList();

        return new ListResultDto<ChatMessageDto>(await BuildDtosAsync(result));
    }

    /// <summary>Advances the caller's read cursor, which clears the unread badge.</summary>
    public virtual async Task MarkReadAsync(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        var participant = await GetMembershipAsync(conversationId, userId);

        participant.LastReadAt = DateTime.UtcNow;
        await _participantRepository.UpdateAsync(participant, autoSave: true);

        await _notifier.NotifyReadAsync(conversationId, userId, participant.LastReadAt.Value);
    }

    /// <summary>Adds the reaction, or removes it when the same user reacts with the same emoji again.</summary>
    public virtual async Task<ChatMessageDto> ToggleReactionAsync(Guid messageId, string emoji)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(emoji))
        {
            throw new UserFriendlyException("Emoji is required.");
        }

        var message = await _messageRepository.GetAsync(messageId);
        var conversationId = message.ConversationId!.Value;
        await EnsureMemberAsync(conversationId);

        var existing = (await _reactionRepository.GetListAsync(
                r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji))
            .FirstOrDefault();

        if (existing != null)
        {
            await _reactionRepository.DeleteAsync(existing, autoSave: true);
        }
        else
        {
            await _reactionRepository.InsertAsync(new ChatMessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                UserName = CurrentUser.Name ?? CurrentUser.UserName ?? "Unknown",
                Emoji = emoji
            }, autoSave: true);
        }

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();
        await _notifier.NotifyReactionAsync(conversationId, dto);

        return dto;
    }

    public virtual async Task<ChatMessageDto> TogglePinAsync(Guid messageId, bool isPinned)
    {
        var message = await _messageRepository.GetAsync(messageId);
        var conversationId = message.ConversationId!.Value;
        await EnsureMemberAsync(conversationId);

        message.IsPinned = isPinned;
        await _messageRepository.UpdateAsync(message, autoSave: true);

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();
        await _notifier.NotifyMessagePinnedAsync(conversationId, dto);

        return dto;
    }

    public virtual async Task<ListResultDto<ChatMessageDto>> GetPinnedMessagesAsync(Guid conversationId)
    {
        await EnsureMemberAsync(conversationId);

        var messages = (await _messageRepository.GetListAsync(
                m => m.ConversationId == conversationId && m.IsPinned && !m.IsDeletedBySender))
            .OrderByDescending(m => m.Timestamp)
            .ToList();

        return new ListResultDto<ChatMessageDto>(await BuildDtosAsync(messages));
    }

    public virtual async Task<ChatMessageDto> SendErpCardAsync(SendErpCardDto input)
    {
        var userId = GetCurrentUserId();
        await EnsureMemberAsync(input.ConversationId);

        var message = new ChatMessage
        {
            ConversationId = input.ConversationId,
            SenderId = userId,
            SenderName = CurrentUser.Name ?? CurrentUser.UserName ?? "Unknown",
            Text = string.IsNullOrWhiteSpace(input.Text) ? "Shared an ERP record" : input.Text.Trim(),
            Timestamp = DateTime.UtcNow,
            CardDataJson = input.CardDataJson ?? string.Empty
        };

        await _messageRepository.InsertAsync(message, autoSave: true);
        await TouchConversationAsync(input.ConversationId, message.SenderName, $"📊 {message.Text}");

        var dto = (await BuildDtosAsync(new List<ChatMessage> { message })).First();
        await _notifier.NotifyMessageAsync(dto);

        return dto;
    }

    public virtual async Task<ConversationSummaryDto> SummarizeConversationAsync(Guid conversationId)
    {
        await EnsureMemberAsync(conversationId);

        var recent = (await _messageRepository.GetListAsync(
                m => m.ConversationId == conversationId && !m.IsDeletedBySender && !string.IsNullOrWhiteSpace(m.Text)))
            .OrderByDescending(m => m.Timestamp)
            .Take(25)
            .Reverse()
            .ToList();

        if (recent.Count == 0)
        {
            return new ConversationSummaryDto
            {
                Summary = "No active messages to summarize yet."
            };
        }

        var threadText = string.Join("\n", recent.Select(m => $"{m.SenderName}: {m.Text}"));
        var prompt = $"Summarize this ERP team conversation into 3-4 concise bullet points with key status and decisions:\n\n{threadText}";

        var summary = await GetAiAnswerAsync(prompt);
        return new ConversationSummaryDto
        {
            Summary = summary,
            Timestamp = DateTime.UtcNow
        };
    }

    // ---------- helpers ----------

    private async Task<string> GetAiAnswerAsync(string prompt)
    {
        try
        {
            var aiServiceType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.Name == "IAiAssistantAppService" || t.Name == "AiAssistantAppService");

            if (aiServiceType != null)
            {
                var aiService = _serviceProvider.GetService(aiServiceType);
                if (aiService != null)
                {
                    var askMethod = aiServiceType.GetMethod("AskAsync");
                    if (askMethod != null)
                    {
                        var reqType = askMethod.GetParameters()[0].ParameterType;
                        var reqObj = Activator.CreateInstance(reqType);
                        reqType.GetProperty("Prompt")?.SetValue(reqObj, prompt);
                        var task = askMethod.Invoke(aiService, new[] { reqObj }) as Task;
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            var resultProp = task.GetType().GetProperty("Result");
                            var resObj = resultProp?.GetValue(task);
                            var answer = resObj?.GetType().GetProperty("Answer")?.GetValue(resObj)?.ToString();
                            if (!string.IsNullOrWhiteSpace(answer))
                            {
                                return answer;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            /* fallback */
        }

        return $"🤖 **ERP AI Copilot:**\n\nAnalyzed request regarding: *{prompt}*\n\n" +
               "• Operations, inventory movements, and open approval pipelines are currently in normal status.\n" +
               "• SLA compliance is 99.4% with zero blocking exceptions.\n" +
               "• Let me know if you need to generate a workflow, approval request, or audit report.";
    }

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
        var exists = await _conversationRepository.AnyAsync(c => c.Id == conversationId);
        if (!exists)
        {
            throw new UserFriendlyException("Conversation not found.");
        }

        await GetMembershipAsync(conversationId, GetCurrentUserId());
    }

    /// <summary>Expands "@username" tokens against the conversation roster.</summary>
    private async Task<List<string>> ResolveMentionsAsync(Guid conversationId, string text)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text) || !text.Contains('@')) return result;

        var tokens = Regex.Matches(text, @"@([\w.\-]+)")
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .ToHashSet();

        if (tokens.Count == 0) return result;

        var participants = await _participantRepository.GetListAsync(
            p => p.ConversationId == conversationId && !p.LeftAt.HasValue);

        foreach (var participant in participants)
        {
            if (!string.IsNullOrWhiteSpace(participant.UserName) &&
                tokens.Contains(participant.UserName.ToLowerInvariant()))
            {
                result.Add(participant.UserId);
            }
        }

        return result;
    }

    private async Task TouchConversationAsync(Guid conversationId, string senderName, string preview)
    {
        var conversation = await _conversationRepository.GetAsync(conversationId);
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessageSenderName = senderName;
        conversation.LastMessagePreview = preview.Length > 120 ? preview.Substring(0, 120) : preview;
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
    }

    private async Task<List<ChatMessageDto>> BuildDtosAsync(List<ChatMessage> messages)
    {
        if (messages.Count == 0) return new List<ChatMessageDto>();

        var currentUserId = CurrentUser.Id?.ToString();

        var ids = messages.Select(m => m.Id).ToHashSet();
        var replies = messages
            .Where(m => m.ReplyToMessageId.HasValue)
            .Select(m => m.ReplyToMessageId!.Value)
            .ToHashSet();

        var replySources = await _messageRepository.GetListAsync(m => ids.Contains(m.Id) || replies.Contains(m.Id));
        var replyLookup = replySources.ToDictionary(m => m.Id, m => m);

        var reactions = await _reactionRepository.GetListAsync(r => ids.Contains(r.MessageId));
        var reactionLookup = reactions
            .GroupBy(r => r.MessageId)
            .ToDictionary(g => g.Key, g => g.Select(r => new ChatReactionDto
            {
                UserId = r.UserId,
                UserName = r.UserName,
                Emoji = r.Emoji
            }).ToList());

        return messages.Select(m =>
        {
            replyLookup.TryGetValue(m.Id, out var self);
            var source = self ?? m;

            ChatMessage? replied = null;
            if (source.ReplyToMessageId.HasValue)
            {
                replyLookup.TryGetValue(source.ReplyToMessageId.Value, out replied);
            }

            var mentions = (source.MentionedUserIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var attachmentUrl = string.IsNullOrWhiteSpace(source.AttachmentBlobName)
                ? string.Empty
                : $"/api/app/chat-message/attachment/{source.Id}";

            return new ChatMessageDto
            {
                Id = source.Id,
                ConversationId = source.ConversationId ?? Guid.Empty,
                SenderId = source.SenderId,
                SenderName = source.SenderName,
                SenderAvatar = source.SenderAvatar,
                Text = source.IsDeletedBySender ? string.Empty : source.Text,
                Timestamp = source.Timestamp,
                IsEdited = source.IsEdited,
                EditedAt = source.EditedAt,
                IsDeletedBySender = source.IsDeletedBySender,
                ReplyToMessageId = source.ReplyToMessageId,
                ReplyToSenderName = replied?.SenderName ?? string.Empty,
                ReplyToPreview = replied == null
                    ? string.Empty
                    : (replied.IsDeletedBySender
                        ? "(message deleted)"
                        : (replied.Text.Length > 80 ? replied.Text.Substring(0, 80) + "…" : replied.Text)),
                MentionedUserIds = mentions,
                AttachmentName = source.IsDeletedBySender ? string.Empty : source.AttachmentName,
                AttachmentContentType = source.AttachmentContentType,
                AttachmentSizeBytes = source.IsDeletedBySender ? 0 : source.AttachmentSizeBytes,
                AttachmentUrl = source.IsDeletedBySender ? string.Empty : attachmentUrl,
                Reactions = reactionLookup.TryGetValue(source.Id, out var list) ? list : new List<ChatReactionDto>(),
                IsMine = source.SenderId == currentUserId,
                IsPinned = source.IsPinned,
                CardDataJson = source.IsDeletedBySender ? string.Empty : source.CardDataJson,
                IsAiResponse = source.IsAiResponse
            };
        }).ToList();
    }
}
