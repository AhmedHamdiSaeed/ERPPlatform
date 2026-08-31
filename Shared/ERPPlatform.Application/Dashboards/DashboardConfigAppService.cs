using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Dashboards
{
    public class DashboardWidgetDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ComponentName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int DefaultWidth { get; set; } = 6;
        public int DefaultHeight { get; set; } = 300;
        public int DefaultOrder { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
    }

    public class UserDashboardConfigDto : EntityDto<Guid>
    {
        public Guid? UserId { get; set; }
        public string DashboardName { get; set; } = "Default";
        public string LayoutJson { get; set; } = "[]";
        public bool IsDefault { get; set; }
    }

    public class SaveDashboardLayoutDto
    {
        public string DashboardName { get; set; } = "Default";
        public string LayoutJson { get; set; } = "[]";
        public bool IsDefault { get; set; } = false;
    }

    public interface IDashboardConfigAppService : IApplicationService
    {
        Task<ListResultDto<DashboardWidgetDto>> GetAvailableWidgetsAsync();
        Task<UserDashboardConfigDto> GetMyDashboardAsync(string dashboardName);
        Task<UserDashboardConfigDto> SaveDashboardAsync(SaveDashboardLayoutDto input);
        Task<ListResultDto<UserDashboardConfigDto>> GetMyDashboardsAsync();
        Task ResetDashboardAsync(string dashboardName);
    }

    public class DashboardConfigAppService : ApplicationService, IDashboardConfigAppService
    {
        private readonly IRepository<DashboardWidget, Guid> _widgetRepository;
        private readonly IRepository<UserDashboardConfig, Guid> _configRepository;

        public DashboardConfigAppService(
            IRepository<DashboardWidget, Guid> widgetRepository,
            IRepository<UserDashboardConfig, Guid> configRepository)
        {
            _widgetRepository = widgetRepository;
            _configRepository = configRepository;
        }

        public async Task<ListResultDto<DashboardWidgetDto>> GetAvailableWidgetsAsync()
        {
            var widgets = await _widgetRepository.GetListAsync();
            var dtos = widgets
                .Where(w => w.IsEnabled)
                .OrderBy(w => w.DefaultOrder)
                .Select(w => new DashboardWidgetDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Title = w.Title,
                    Description = w.Description,
                    Category = w.Category,
                    ComponentName = w.ComponentName,
                    Icon = w.Icon,
                    DefaultWidth = w.DefaultWidth,
                    DefaultHeight = w.DefaultHeight,
                    DefaultOrder = w.DefaultOrder,
                    IsEnabled = w.IsEnabled
                }).ToList();
            return new ListResultDto<DashboardWidgetDto>(dtos);
        }

        public async Task<UserDashboardConfigDto> GetMyDashboardAsync(string dashboardName)
        {
            var userId = CurrentUser.Id;
            var configs = await _configRepository.GetListAsync();
            var config = configs.FirstOrDefault(c =>
                c.UserId == userId &&
                c.DashboardName == (string.IsNullOrWhiteSpace(dashboardName) ? "Default" : dashboardName));

            if (config == null)
            {
                // Return a default empty layout
                return new UserDashboardConfigDto
                {
                    UserId = userId,
                    DashboardName = string.IsNullOrWhiteSpace(dashboardName) ? "Default" : dashboardName,
                    LayoutJson = "[]",
                    IsDefault = string.IsNullOrWhiteSpace(dashboardName) || dashboardName == "Default"
                };
            }

            return new UserDashboardConfigDto
            {
                Id = config.Id,
                UserId = config.UserId,
                DashboardName = config.DashboardName,
                LayoutJson = config.LayoutJson,
                IsDefault = config.IsDefault
            };
        }

        public async Task<UserDashboardConfigDto> SaveDashboardAsync(SaveDashboardLayoutDto input)
        {
            var userId = CurrentUser.Id;
            var dashboardName = string.IsNullOrWhiteSpace(input.DashboardName) ? "Default" : input.DashboardName;

            var configs = await _configRepository.GetListAsync();
            var existing = configs.FirstOrDefault(c =>
                c.UserId == userId && c.DashboardName == dashboardName);

            if (existing == null)
            {
                var newConfig = new UserDashboardConfig
                {
                    UserId = userId,
                    DashboardName = dashboardName,
                    LayoutJson = input.LayoutJson,
                    IsDefault = input.IsDefault
                };
                await _configRepository.InsertAsync(newConfig, autoSave: true);

                return new UserDashboardConfigDto
                {
                    Id = newConfig.Id,
                    UserId = newConfig.UserId,
                    DashboardName = newConfig.DashboardName,
                    LayoutJson = newConfig.LayoutJson,
                    IsDefault = newConfig.IsDefault
                };
            }
            else
            {
                existing.LayoutJson = input.LayoutJson;
                existing.IsDefault = input.IsDefault;
                await _configRepository.UpdateAsync(existing);

                return new UserDashboardConfigDto
                {
                    Id = existing.Id,
                    UserId = existing.UserId,
                    DashboardName = existing.DashboardName,
                    LayoutJson = existing.LayoutJson,
                    IsDefault = existing.IsDefault
                };
            }
        }

        public async Task<ListResultDto<UserDashboardConfigDto>> GetMyDashboardsAsync()
        {
            var userId = CurrentUser.Id;
            var configs = await _configRepository.GetListAsync();
            var dtos = configs
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsDefault)
                .ThenBy(c => c.DashboardName)
                .Select(c => new UserDashboardConfigDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    DashboardName = c.DashboardName,
                    LayoutJson = c.LayoutJson,
                    IsDefault = c.IsDefault
                }).ToList();
            return new ListResultDto<UserDashboardConfigDto>(dtos);
        }

        public async Task ResetDashboardAsync(string dashboardName)
        {
            var userId = CurrentUser.Id;
            var configs = await _configRepository.GetListAsync();
            var config = configs.FirstOrDefault(c =>
                c.UserId == userId &&
                c.DashboardName == (string.IsNullOrWhiteSpace(dashboardName) ? "Default" : dashboardName));

            if (config != null)
            {
                config.LayoutJson = "[]";
                await _configRepository.UpdateAsync(config);
            }
        }
    }
}
