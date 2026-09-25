using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace ERPPlatform.Application.Tenants
{
    public class TenantProfileDto : EntityDto<Guid>
    {
        public Guid TenantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string CustomDomain { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string Timezone { get; set; } = "UTC";
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#4f46e5";
        public string PlanTier { get; set; } = "Starter";
        public string Status { get; set; } = "Active";
        public int MaxUsers { get; set; } = 10;
        public int StorageLimitGb { get; set; } = 10;
        public double UsedStorageMb { get; set; } = 120.5;
        public int ActiveUserCount { get; set; } = 1;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminPhone { get; set; } = string.Empty;
        public bool IsDedicatedDb { get; set; }
        public string CustomConnectionString { get; set; } = string.Empty;
        public List<string> EnabledModules { get; set; } = new();
        public DateTime? TrialEndDate { get; set; }
        public DateTime? SubscriptionRenewalDate { get; set; }
        public decimal MonthlyFee { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateTenantInputDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string Timezone { get; set; } = "UTC";
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#4f46e5";
        public string PlanTier { get; set; } = "Starter"; // Trial, Starter, Professional, Enterprise
        public int MaxUsers { get; set; } = 10;
        public int StorageLimitGb { get; set; } = 10;
        public bool IsDedicatedDb { get; set; } = false;
        public string CustomConnectionString { get; set; } = string.Empty;
        public List<string> EnabledModules { get; set; } = new() { "HR", "Finance", "Sales", "Inventory", "AI", "Workflow" };
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string AdminPhone { get; set; } = string.Empty;
        public bool SendWelcomeEmail { get; set; } = true;
    }

    public class UpdateTenantProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string Timezone { get; set; } = "UTC";
        public string PlanTier { get; set; } = "Starter";
        public string Status { get; set; } = "Active";
        public int MaxUsers { get; set; } = 10;
        public int StorageLimitGb { get; set; } = 10;
        public bool IsDedicatedDb { get; set; }
        public string CustomConnectionString { get; set; } = string.Empty;
        public List<string> EnabledModules { get; set; } = new();
    }

    public class TenantStatsSummaryDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TrialTenants { get; set; }
        public int SuspendedTenants { get; set; }
        public decimal TotalMRR { get; set; }
        public int TotalUsersAcrossTenants { get; set; }
        public double TotalStorageAllocatedGb { get; set; }
    }

    public class TenantImpersonationResultDto
    {
        public bool Success { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string ImpersonationToken { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public interface ITenantManagementAppService : IApplicationService
    {
        Task<List<TenantProfileDto>> GetTenantListAsync(string? filter = null, string? status = null, string? planTier = null);
        Task<TenantProfileDto> GetTenantByIdAsync(Guid id);
        Task<TenantProfileDto> CreateTenantAsync(CreateTenantInputDto input);
        Task<TenantProfileDto> UpdateTenantAsync(Guid id, UpdateTenantProfileDto input);
        Task DeleteTenantAsync(Guid id);
        Task SetStatusAsync(Guid id, string status);
        Task SetConnectionStringAsync(Guid id, string connectionString);
        Task ToggleModuleAsync(Guid id, string moduleName, bool isEnabled);
        Task<TenantImpersonationResultDto> ImpersonateTenantAsync(Guid id);
        Task<TenantStatsSummaryDto> GetStatsSummaryAsync();
    }

    public class TenantManagementAppService : ApplicationService, ITenantManagementAppService
    {
        private readonly IRepository<TenantProfile, Guid> _profileRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantManager _tenantManager;

        public TenantManagementAppService(
            IRepository<TenantProfile, Guid> profileRepository,
            ITenantRepository tenantRepository,
            ITenantManager tenantManager)
        {
            _profileRepository = profileRepository;
            _tenantRepository = tenantRepository;
            _tenantManager = tenantManager;
        }

        public async Task<List<TenantProfileDto>> GetTenantListAsync(string? filter = null, string? status = null, string? planTier = null)
        {
            var query = await _profileRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = filter.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(f) || x.Code.ToLower().Contains(f) || x.Subdomain.ToLower().Contains(f) || x.AdminEmail.ToLower().Contains(f));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(planTier) && planTier != "ALL")
            {
                query = query.Where(x => x.PlanTier == planTier);
            }

            var list = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime));
            
            // If empty, return default seeded sample profiles
            if (list.Count == 0)
            {
                var defaultProfiles = GetSampleProfiles();
                foreach (var p in defaultProfiles)
                {
                    await _profileRepository.InsertAsync(p);
                }
                list = defaultProfiles;
            }

            return list.Select(MapToDto).ToList();
        }

        public async Task<TenantProfileDto> GetTenantByIdAsync(Guid id)
        {
            var profile = await _profileRepository.GetAsync(id);
            return MapToDto(profile);
        }

        public async Task<TenantProfileDto> CreateTenantAsync(CreateTenantInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException("Tenant organization name is required.");
            }

            var code = string.IsNullOrWhiteSpace(input.Code) ? input.Name.Trim().ToLowerInvariant().Replace(" ", "-") : input.Code.Trim().ToLowerInvariant();
            var subdomain = string.IsNullOrWhiteSpace(input.Subdomain) ? code : input.Subdomain.Trim().ToLowerInvariant();

            // Create ABP Core Tenant
            var abpTenant = await _tenantManager.CreateAsync(input.Name);
            await _tenantRepository.InsertAsync(abpTenant);

            decimal fee = input.PlanTier switch
            {
                "Starter" => 99m,
                "Professional" => 299m,
                "Enterprise" => 899m,
                _ => 0m
            };

            var profile = new TenantProfile
            {
                TenantId = abpTenant.Id,
                Code = code,
                Name = input.Name,
                LegalName = string.IsNullOrWhiteSpace(input.LegalName) ? input.Name : input.LegalName,
                Subdomain = subdomain,
                TaxNumber = input.TaxNumber,
                Currency = string.IsNullOrWhiteSpace(input.Currency) ? "USD" : input.Currency,
                Timezone = string.IsNullOrWhiteSpace(input.Timezone) ? "UTC" : input.Timezone,
                LogoUrl = string.IsNullOrWhiteSpace(input.LogoUrl) ? "https://images.unsplash.com/photo-1572021335469-31706a17aaef?w=150" : input.LogoUrl,
                PrimaryColor = string.IsNullOrWhiteSpace(input.PrimaryColor) ? "#4f46e5" : input.PrimaryColor,
                PlanTier = input.PlanTier,
                Status = input.PlanTier == "Trial" ? "Trial" : "Active",
                MaxUsers = input.MaxUsers > 0 ? input.MaxUsers : 10,
                StorageLimitGb = input.StorageLimitGb > 0 ? input.StorageLimitGb : 10,
                UsedStorageMb = 45.0,
                ActiveUserCount = 1,
                AdminEmail = input.AdminEmail,
                AdminFullName = input.AdminFullName,
                AdminPhone = input.AdminPhone,
                IsDedicatedDb = input.IsDedicatedDb,
                CustomConnectionString = input.CustomConnectionString,
                EnabledModulesJson = JsonSerializer.Serialize(input.EnabledModules ?? new List<string> { "HR", "Finance", "Sales", "Inventory", "AI", "Workflow" }),
                TrialEndDate = input.PlanTier == "Trial" ? DateTime.UtcNow.AddDays(14) : null,
                SubscriptionRenewalDate = DateTime.UtcNow.AddMonths(1),
                MonthlyFee = fee
            };

            await _profileRepository.InsertAsync(profile);

            return MapToDto(profile);
        }

        public async Task<TenantProfileDto> UpdateTenantAsync(Guid id, UpdateTenantProfileDto input)
        {
            var profile = await _profileRepository.GetAsync(id);
            profile.Name = input.Name;
            profile.LegalName = input.LegalName;
            profile.TaxNumber = input.TaxNumber;
            profile.Currency = input.Currency;
            profile.Timezone = input.Timezone;
            profile.PlanTier = input.PlanTier;
            profile.Status = input.Status;
            profile.MaxUsers = input.MaxUsers;
            profile.StorageLimitGb = input.StorageLimitGb;
            profile.IsDedicatedDb = input.IsDedicatedDb;
            profile.CustomConnectionString = input.CustomConnectionString;
            if (input.EnabledModules != null)
            {
                profile.EnabledModulesJson = JsonSerializer.Serialize(input.EnabledModules);
            }

            await _profileRepository.UpdateAsync(profile);
            return MapToDto(profile);
        }

        public async Task DeleteTenantAsync(Guid id)
        {
            var profile = await _profileRepository.GetAsync(id);
            await _profileRepository.DeleteAsync(profile);

            var abpTenant = await _tenantRepository.FindAsync(profile.TenantId);
            if (abpTenant != null)
            {
                await _tenantRepository.DeleteAsync(abpTenant);
            }
        }

        public async Task SetStatusAsync(Guid id, string status)
        {
            var profile = await _profileRepository.GetAsync(id);
            profile.Status = status;
            await _profileRepository.UpdateAsync(profile);
        }

        public async Task SetConnectionStringAsync(Guid id, string connectionString)
        {
            var profile = await _profileRepository.GetAsync(id);
            profile.IsDedicatedDb = !string.IsNullOrWhiteSpace(connectionString);
            profile.CustomConnectionString = connectionString;
            await _profileRepository.UpdateAsync(profile);
        }

        public async Task ToggleModuleAsync(Guid id, string moduleName, bool isEnabled)
        {
            var profile = await _profileRepository.GetAsync(id);
            var modules = ParseModules(profile.EnabledModulesJson);
            if (isEnabled && !modules.Contains(moduleName))
            {
                modules.Add(moduleName);
            }
            else if (!isEnabled && modules.Contains(moduleName))
            {
                modules.Remove(moduleName);
            }

            profile.EnabledModulesJson = JsonSerializer.Serialize(modules);
            await _profileRepository.UpdateAsync(profile);
        }

        public async Task<TenantImpersonationResultDto> ImpersonateTenantAsync(Guid id)
        {
            var profile = await _profileRepository.GetAsync(id);
            var token = $"impersonate_{profile.TenantId:N}_{Guid.NewGuid():N}";

            return new TenantImpersonationResultDto
            {
                Success = true,
                TenantId = profile.TenantId,
                TenantName = profile.Name,
                ImpersonationToken = token,
                RedirectUrl = $"/?__tenant={profile.Subdomain}",
                Message = $"Diagnostic impersonation session created for tenant '{profile.Name}'."
            };
        }

        public async Task<TenantStatsSummaryDto> GetStatsSummaryAsync()
        {
            var list = await _profileRepository.GetListAsync();
            if (list.Count == 0)
            {
                list = GetSampleProfiles();
            }

            return new TenantStatsSummaryDto
            {
                TotalTenants = list.Count,
                ActiveTenants = list.Count(x => x.Status == "Active"),
                TrialTenants = list.Count(x => x.Status == "Trial"),
                SuspendedTenants = list.Count(x => x.Status == "Suspended"),
                TotalMRR = list.Where(x => x.Status == "Active").Sum(x => x.MonthlyFee),
                TotalUsersAcrossTenants = list.Sum(x => x.ActiveUserCount),
                TotalStorageAllocatedGb = list.Sum(x => x.StorageLimitGb)
            };
        }

        private static TenantProfileDto MapToDto(TenantProfile entity)
        {
            return new TenantProfileDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Code = entity.Code,
                Name = entity.Name,
                LegalName = entity.LegalName,
                Subdomain = entity.Subdomain,
                CustomDomain = entity.CustomDomain,
                TaxNumber = entity.TaxNumber,
                Currency = entity.Currency,
                Timezone = entity.Timezone,
                LogoUrl = entity.LogoUrl,
                PrimaryColor = entity.PrimaryColor,
                PlanTier = entity.PlanTier,
                Status = entity.Status,
                MaxUsers = entity.MaxUsers,
                StorageLimitGb = entity.StorageLimitGb,
                UsedStorageMb = entity.UsedStorageMb,
                ActiveUserCount = entity.ActiveUserCount,
                AdminEmail = entity.AdminEmail,
                AdminFullName = entity.AdminFullName,
                AdminPhone = entity.AdminPhone,
                IsDedicatedDb = entity.IsDedicatedDb,
                CustomConnectionString = entity.CustomConnectionString,
                EnabledModules = ParseModules(entity.EnabledModulesJson),
                TrialEndDate = entity.TrialEndDate,
                SubscriptionRenewalDate = entity.SubscriptionRenewalDate,
                MonthlyFee = entity.MonthlyFee,
                CreationTime = entity.CreationTime
            };
        }

        private static List<string> ParseModules(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string> { "HR", "Finance", "Sales", "Inventory", "AI", "Workflow" };
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string> { "HR", "Finance", "Sales", "Inventory", "AI", "Workflow" };
            }
        }

        private static List<TenantProfile> GetSampleProfiles()
        {
            return new List<TenantProfile>
            {
                new TenantProfile
                {
                    TenantId = Guid.NewGuid(),
                    Code = "al-madina-trading",
                    Name = "Al-Madina Global Trading",
                    LegalName = "Al-Madina Global Trading LLC",
                    Subdomain = "almadina",
                    TaxNumber = "VAT-99281-SA",
                    Currency = "SAR",
                    Timezone = "Asia/Riyadh",
                    LogoUrl = "https://images.unsplash.com/photo-1572021335469-31706a17aaef?w=150",
                    PrimaryColor = "#059669",
                    PlanTier = "Enterprise",
                    Status = "Active",
                    MaxUsers = 150,
                    StorageLimitGb = 100,
                    UsedStorageMb = 14200.0,
                    ActiveUserCount = 48,
                    AdminEmail = "admin@almadina-trading.com",
                    AdminFullName = "Tariq Al-Mansoor",
                    AdminPhone = "+966 50 123 4567",
                    IsDedicatedDb = true,
                    CustomConnectionString = "Server=db-dedicated-01.internal;Database=ERP_AlMadina;User Id=erp_user;Password=***;",
                    EnabledModulesJson = "[\"HR\",\"Finance\",\"Sales\",\"Inventory\",\"Manufacturing\",\"AI\",\"Workflow\"]",
                    MonthlyFee = 899m
                },
                new TenantProfile
                {
                    TenantId = Guid.NewGuid(),
                    Code = "apex-tech-solutions",
                    Name = "Apex Technology Solutions",
                    LegalName = "Apex Solutions FZCO",
                    Subdomain = "apex",
                    TaxNumber = "AE-8840192",
                    Currency = "USD",
                    Timezone = "Asia/Dubai",
                    LogoUrl = "https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=150",
                    PrimaryColor = "#4f46e5",
                    PlanTier = "Professional",
                    Status = "Active",
                    MaxUsers = 50,
                    StorageLimitGb = 30,
                    UsedStorageMb = 4850.0,
                    ActiveUserCount = 22,
                    AdminEmail = "sysadmin@apexsolutions.io",
                    AdminFullName = "Laila Mahmoud",
                    AdminPhone = "+971 4 999 8888",
                    IsDedicatedDb = false,
                    EnabledModulesJson = "[\"HR\",\"Finance\",\"Sales\",\"Inventory\",\"AI\",\"Workflow\"]",
                    MonthlyFee = 299m
                },
                new TenantProfile
                {
                    TenantId = Guid.NewGuid(),
                    Code = "cairo-logistics-hub",
                    Name = "Cairo Express Logistics",
                    LegalName = "Cairo Express Logistics S.A.E.",
                    Subdomain = "cairoexpress",
                    TaxNumber = "EG-TR-44819",
                    Currency = "EGP",
                    Timezone = "Africa/Cairo",
                    LogoUrl = "https://images.unsplash.com/photo-1542744173-8e7e53415bb0?w=150",
                    PrimaryColor = "#d97706",
                    PlanTier = "Starter",
                    Status = "Trial",
                    MaxUsers = 15,
                    StorageLimitGb = 10,
                    UsedStorageMb = 820.0,
                    ActiveUserCount = 5,
                    AdminEmail = "karim@cairoexpress.eg",
                    AdminFullName = "Karim El-Sayed",
                    AdminPhone = "+20 100 555 1234",
                    IsDedicatedDb = false,
                    EnabledModulesJson = "[\"Finance\",\"Sales\",\"Inventory\",\"Workflow\"]",
                    TrialEndDate = DateTime.UtcNow.AddDays(9),
                    MonthlyFee = 99m
                }
            };
        }
    }
}
