using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DemoTenantsDataSeedContributor> _logger;

    public DemoTenantsDataSeedContributor(
        ITenantRepository tenantRepository,
        ITenantManager tenantManager,
        ICurrentTenant currentTenant,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        ILogger<DemoTenantsDataSeedContributor> logger)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _currentTenant = currentTenant;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
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
            try
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
                else if (string.IsNullOrWhiteSpace(tenant.GetProperty<string>("LogoUrl")))
                {
                    tenant.SetProperty("LogoUrl", def.LogoUrl);
                    tenant.SetProperty("PrimaryColor", "#2563eb");
                    tenant.SetProperty("SupportEmail", def.AdminEmail);
                    tenant.SetProperty("WebsiteUrl", $"https://{def.Name.ToLower()}.erpplatform.com");
                    await _tenantRepository.UpdateAsync(tenant);
                }

                using (_currentTenant.Change(tenant.Id))
                {
                    // Resolve (or create) the tenant-scoped "admin" role. Create explicitly and fall
                    // back to a re-fetch if it already exists, so we never hand AddToRoleAsync a null
                    // or mismatched role. Previous code relied solely on FindByNameAsync, which could
                    // surface the host role and then throw "Role ADMIN does not exist!" on assignment.
                    var adminRole = await _roleManager.FindByNameAsync("admin");
                    if (adminRole == null)
                    {
                        var created = new IdentityRole(Guid.NewGuid(), "admin");
                        var roleResult = await _roleManager.CreateAsync(created);
                        adminRole = roleResult.Succeeded
                            ? created
                            : await _roleManager.FindByNameAsync("admin");
                    }

                    if (adminRole == null)
                    {
                        _logger.LogWarning(
                            "Could not resolve or create the 'admin' role for tenant {Tenant}; skipping admin-user seeding for it.",
                            def.Name);
                        continue;
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
                            await _userManager.AddToRoleAsync(adminUser, adminRole.Name);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to create admin user for tenant {Tenant}: {Errors}",
                                def.Name,
                                string.Join("; ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else if (!await _userManager.IsInRoleAsync(adminUser, adminRole.Name))
                    {
                        await _userManager.AddToRoleAsync(adminUser, adminRole.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                // Demo-tenant seeding is best-effort: never abort the whole application seed/startup.
                _logger.LogWarning(ex, "Failed to seed demo tenant {TenantName}.", def.Name);
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
