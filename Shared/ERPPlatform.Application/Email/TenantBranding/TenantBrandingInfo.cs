using System;

namespace ERPPlatform.Email.TenantBranding;

public class TenantBrandingInfo
{
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = "ERP Platform";
    public string LogoUrl { get; set; } = "https://cdn-icons-png.flaticon.com/512/9068/9068642.png";
    public string SupportEmail { get; set; } = "support@erpplatform.com";
    public string WebsiteUrl { get; set; } = "https://erpplatform.com";
    public string PrimaryColor { get; set; } = "#2563eb";
}
