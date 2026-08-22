using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Chat
{
    public class ChatMessageDto : EntityDto<Guid>
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class SendChatMessageDto
    {
        public string SenderId { get; set; } = "usr-001";
        public string SenderName { get; set; } = "Ahmed Hamdi";
        public string SenderAvatar { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public interface IChatAppService : IApplicationService
    {
        Task<ListResultDto<ChatMessageDto>> GetChannelMessagesAsync(string channelName);
        Task<ListResultDto<ChatMessageDto>> GetDirectMessagesAsync(string userId, string otherUserId);
        Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto input);
    }

    public class ChatAppService : ApplicationService, IChatAppService
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepository;

        public ChatAppService(IRepository<ChatMessage, Guid> chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ListResultDto<ChatMessageDto>> GetChannelMessagesAsync(string channelName)
        {
            var messages = await _chatRepository.GetListAsync();
            var filtered = messages
                .Where(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    SenderAvatar = m.SenderAvatar,
                    ReceiverId = m.ReceiverId,
                    ChannelName = m.ChannelName,
                    Text = m.Text,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead
                }).ToList();

            return new ListResultDto<ChatMessageDto>(filtered);
        }

        public async Task<ListResultDto<ChatMessageDto>> GetDirectMessagesAsync(string userId, string otherUserId)
        {
            var messages = await _chatRepository.GetListAsync();
            var filtered = messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    SenderAvatar = m.SenderAvatar,
                    ReceiverId = m.ReceiverId,
                    ChannelName = m.ChannelName,
                    Text = m.Text,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead
                }).ToList();

            return new ListResultDto<ChatMessageDto>(filtered);
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto input)
        {
            var msg = new ChatMessage
            {
                SenderId = string.IsNullOrWhiteSpace(input.SenderId) ? "usr-001" : input.SenderId,
                SenderName = string.IsNullOrWhiteSpace(input.SenderName) ? "Ahmed Hamdi" : input.SenderName,
                SenderAvatar = input.SenderAvatar,
                ReceiverId = input.ReceiverId,
                ChannelName = input.ChannelName,
                Text = input.Text,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await _chatRepository.InsertAsync(msg);

            return new ChatMessageDto
            {
                Id = msg.Id,
                SenderId = msg.SenderId,
                SenderName = msg.SenderName,
                SenderAvatar = msg.SenderAvatar,
                ReceiverId = msg.ReceiverId,
                ChannelName = msg.ChannelName,
                Text = msg.Text,
                Timestamp = msg.Timestamp,
                IsRead = msg.IsRead
            };
        }
    }
}
