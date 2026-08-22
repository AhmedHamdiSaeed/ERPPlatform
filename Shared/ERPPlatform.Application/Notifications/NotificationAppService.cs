using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Notifications
{
    public class NotificationDto : EntityDto<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "System";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class CreateNotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "System";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }

    public interface INotificationAppService : IApplicationService
    {
        Task<ListResultDto<NotificationDto>> GetNotificationsAsync();
        Task<NotificationDto> SendNotificationAsync(CreateNotificationDto input);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync();
    }

    public class NotificationAppService : ApplicationService, INotificationAppService
    {
        private readonly IRepository<SystemNotification, Guid> _notificationRepository;

        public NotificationAppService(IRepository<SystemNotification, Guid> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ListResultDto<NotificationDto>> GetNotificationsAsync()
        {
            var notifications = await _notificationRepository.GetListAsync();
            var dtos = notifications
                .OrderByDescending(n => n.Timestamp)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    Link = n.Link,
                    Timestamp = n.Timestamp,
                    IsRead = n.IsRead
                }).ToList();

            return new ListResultDto<NotificationDto>(dtos);
        }

        public async Task<NotificationDto> SendNotificationAsync(CreateNotificationDto input)
        {
            var notif = new SystemNotification
            {
                UserId = input.UserId,
                Type = string.IsNullOrWhiteSpace(input.Type) ? "System" : input.Type,
                Title = input.Title,
                Message = input.Message,
                Link = input.Link,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.InsertAsync(notif);

            return new NotificationDto
            {
                Id = notif.Id,
                UserId = notif.UserId,
                Type = notif.Type,
                Title = notif.Title,
                Message = notif.Message,
                Link = notif.Link,
                Timestamp = notif.Timestamp,
                IsRead = notif.IsRead
            };
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notif = await _notificationRepository.GetAsync(id);
            notif.IsRead = true;
            await _notificationRepository.UpdateAsync(notif);
        }

        public async Task MarkAllAsReadAsync()
        {
            var notifications = await _notificationRepository.GetListAsync();
            foreach (var notif in notifications.Where(n => !n.IsRead))
            {
                notif.IsRead = true;
                await _notificationRepository.UpdateAsync(notif);
            }
        }
    }
}
