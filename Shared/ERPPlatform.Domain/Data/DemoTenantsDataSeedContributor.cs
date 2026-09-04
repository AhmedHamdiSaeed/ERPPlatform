using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace ERPPlatform.Data;

public class DemoTenantsDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantManager _tenantManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    public DemoTenantsDataSeedContributor(
        ITenantRepository tenantRepository,
        ITenantManager tenantManager,
        ICurrentTenant currentTenant,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _currentTenant = currentTenant;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private static readonly List<DemoTenantDefinition> DemoTenants = new()
    {
        new("Acme", "https://images.unsplash.com/photo-1599305445671-ac291c95aaa9?w=200", "admin@acme.com", "Admin123!", "Acme Admin"),
        new("TechFlow", "https://images.unsplash.com/photo-1516876437184-593fda40c7ce?w=200", "admin@techflow.com", "Admin123!", "TechFlow Admin"),
        new("AlAmal", "https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=200", "admin@alamal.com", "Admin123!", "AlAmal Admin")
    };

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId != null)
        {
            return;
        }

        foreach (var def in DemoTenants)
        {
            var tenant = await _tenantRepository.FindByNameAsync(def.Name);
            if (tenant == null)
            {
                tenant = await _tenantManager.CreateAsync(def.Name);
                tenant.SetProperty("LogoUrl", def.LogoUrl);
                tenant.SetProperty("PrimaryColor", "#2563eb");
                tenant.SetProperty("SupportEmail", def.AdminEmail);
                tenant.SetProperty("WebsiteUrl", $"https://{def.Name.ToLower()}.erpplatform.com");
                await _tenantRepository.InsertAsync(tenant);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(tenant.GetProperty<string>("LogoUrl")))
                {
                    tenant.SetProperty("LogoUrl", def.LogoUrl);
                    tenant.SetProperty("PrimaryColor", "#2563eb");
                    tenant.SetProperty("SupportEmail", def.AdminEmail);
                    tenant.SetProperty("WebsiteUrl", $"https://{def.Name.ToLower()}.erpplatform.com");
                    await _tenantRepository.UpdateAsync(tenant);
                }
            }

            using (_currentTenant.Change(tenant.Id))
            {
                if (await _roleManager.FindByNameAsync("admin") == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), "admin"));
                }

                var adminUser = await _userManager.FindByEmailAsync(def.AdminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser(Guid.NewGuid(), def.AdminEmail, def.AdminEmail, tenant.Id)
                    {
                        Name = def.AdminName,
                        Surname = "Administrator"
                    };
                    var result = await _userManager.CreateAsync(adminUser, def.AdminPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "admin");
                    }
                }
                else
                {
                    if (!await _userManager.IsInRoleAsync(adminUser, "admin"))
                    {
                        await _userManager.AddToRoleAsync(adminUser, "admin");
                    }
                }
            }
        }
    }

    private sealed record DemoTenantDefinition(
        string Name,
        string LogoUrl,
        string AdminEmail,
        string AdminPassword,
        string AdminName);
}
