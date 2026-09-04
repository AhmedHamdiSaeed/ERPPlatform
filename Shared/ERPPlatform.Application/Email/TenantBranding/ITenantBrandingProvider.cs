using System;
using System.Threading.Tasks;

namespace ERPPlatform.Email.TenantBranding;

public interface ITenantBrandingProvider
{
    Task<TenantBrandingInfo> GetBrandingAsync(Guid? tenantId = null, string? tenantName = null);
    Task SetTenantLogoAsync(Guid tenantId, string logoUrl);
}
