using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Notifications
{
    public class DeviceRegistrationDto : EntityDto<Guid>
    {
        public Guid? UserId { get; set; }
        public string DeviceToken { get; set; } = string.Empty;
        public string Platform { get; set; } = "Android";
        public string DeviceName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public DateTime? LastRegisteredAt { get; set; }
    }

    public class RegisterDeviceDto
    {
        public string DeviceToken { get; set; } = string.Empty;
        public string Platform { get; set; } = "Android";
        public string DeviceName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
    }

    public interface IDeviceRegistrationAppService : IApplicationService
    {
        Task<DeviceRegistrationDto> RegisterAsync(RegisterDeviceDto input);
        Task UnregisterAsync(Guid id);
        Task UpdateTokenAsync(Guid id, string newToken);
        Task<ListResultDto<DeviceRegistrationDto>> GetMyDevicesAsync();
    }

    public class DeviceRegistrationAppService : ApplicationService, IDeviceRegistrationAppService
    {
        private readonly IRepository<DeviceRegistration, Guid> _deviceRepository;

        public DeviceRegistrationAppService(IRepository<DeviceRegistration, Guid> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<DeviceRegistrationDto> RegisterAsync(RegisterDeviceDto input)
        {
            // Check if device token already exists; if so, update it
            var devices = await _deviceRepository.GetListAsync();
            var existing = devices.FirstOrDefault(d => d.DeviceToken == input.DeviceToken);

            if (existing != null)
            {
                existing.UserId = CurrentUser.Id;
                existing.Platform = input.Platform;
                existing.DeviceName = input.DeviceName;
                existing.AppVersion = input.AppVersion;
                existing.OsVersion = input.OsVersion;
                existing.IsEnabled = true;
                existing.LastRegisteredAt = DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(existing);

                return new DeviceRegistrationDto
                {
                    Id = existing.Id,
                    UserId = existing.UserId,
                    DeviceToken = existing.DeviceToken,
                    Platform = existing.Platform,
                    DeviceName = existing.DeviceName,
                    AppVersion = existing.AppVersion,
                    OsVersion = existing.OsVersion,
                    IsEnabled = existing.IsEnabled,
                    LastRegisteredAt = existing.LastRegisteredAt
                };
            }

            var entity = new DeviceRegistration
            {
                UserId = CurrentUser.Id,
                DeviceToken = input.DeviceToken,
                Platform = input.Platform,
                DeviceName = input.DeviceName,
                AppVersion = input.AppVersion,
                OsVersion = input.OsVersion,
                IsEnabled = true,
                LastRegisteredAt = DateTime.UtcNow
            };

            await _deviceRepository.InsertAsync(entity);

            return new DeviceRegistrationDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                DeviceToken = entity.DeviceToken,
                Platform = entity.Platform,
                DeviceName = entity.DeviceName,
                AppVersion = entity.AppVersion,
                OsVersion = entity.OsVersion,
                IsEnabled = entity.IsEnabled,
                LastRegisteredAt = entity.LastRegisteredAt
            };
        }

        public async Task UnregisterAsync(Guid id)
        {
            var device = await _deviceRepository.GetAsync(id);
            device.IsEnabled = false;
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task UpdateTokenAsync(Guid id, string newToken)
        {
            var device = await _deviceRepository.GetAsync(id);
            device.DeviceToken = newToken;
            device.LastRegisteredAt = DateTime.UtcNow;
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task<ListResultDto<DeviceRegistrationDto>> GetMyDevicesAsync()
        {
            var userId = CurrentUser.Id;
            var devices = await _deviceRepository.GetListAsync();
            var dtos = devices
                .Where(d => d.UserId == userId && d.IsEnabled)
                .Select(d => new DeviceRegistrationDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    DeviceToken = d.DeviceToken,
                    Platform = d.Platform,
                    DeviceName = d.DeviceName,
                    AppVersion = d.AppVersion,
                    OsVersion = d.OsVersion,
                    IsEnabled = d.IsEnabled,
                    LastRegisteredAt = d.LastRegisteredAt
                }).ToList();

            return new ListResultDto<DeviceRegistrationDto>(dtos);
        }
    }
}
