using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace ERPPlatform.Email.TenantBranding;

public class TenantBrandingProvider : ITenantBrandingProvider, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    public const string DefaultLogoUrl = "https://cdn-icons-png.flaticon.com/512/9068/9068642.png";
    public const string DefaultSystemName = "ERP Platform Enterprise";

    public TenantBrandingProvider(
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant)
    {
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
    }

    public async Task<TenantBrandingInfo> GetBrandingAsync(Guid? tenantId = null, string? tenantName = null)
    {
        var targetId = tenantId ?? _currentTenant.Id;
        Tenant? tenant = null;

        if (targetId.HasValue)
        {
            tenant = await _tenantRepository.FindAsync(targetId.Value, includeDetails: true);
        }
        else if (!string.IsNullOrWhiteSpace(tenantName))
        {
            tenant = await _tenantRepository.FindByNameAsync(tenantName, includeDetails: true);
        }

        if (tenant == null)
        {
            return new TenantBrandingInfo
            {
                TenantId = null,
                TenantName = DefaultSystemName,
                LogoUrl = DefaultLogoUrl,
                SupportEmail = "support@erpplatform.com",
                WebsiteUrl = "https://erpplatform.com",
                PrimaryColor = "#2563eb"
            };
        }

        var customLogo = tenant.GetProperty<string>("LogoUrl");
        var logoUrl = !string.IsNullOrWhiteSpace(customLogo) ? customLogo : DefaultLogoUrl;

        return new TenantBrandingInfo
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            LogoUrl = logoUrl,
            SupportEmail = $"support@{tenant.Name.ToLower().Replace(" ", "")}.com",
            WebsiteUrl = $"https://{tenant.Name.ToLower().Replace(" ", "")}.erpplatform.com",
            PrimaryColor = "#2563eb"
        };
    }

    public async Task SetTenantLogoAsync(Guid tenantId, string logoUrl)
    {
        var tenant = await _tenantRepository.GetAsync(tenantId);
        tenant.SetProperty("LogoUrl", logoUrl);
        await _tenantRepository.UpdateAsync(tenant);
    }
}
