using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace ERPPlatform.Data;

/// <summary>
/// Seeds a handful of demo accounts so the login screen can be exercised with
/// more than just the admin user. Runs idempotently - it skips any user that
/// already exists, so re-running the migrator is safe.
///
/// Crucially, it also guarantees the admin account is wired to the built-in
/// "admin" role. Without that role membership every permission-gated endpoint
/// (including the RBAC module's own permission/role APIs) returns 403 and the
/// Roles &amp; Permissions screen can't load. The standard ABP identity seed in
/// this project did not reliably assign the role, so we do it explicitly here.
///
/// Password policy requires a digit, so every demo password ends in a digit.
/// </summary>
public class DemoUsersDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    public DemoUsersDataSeedContributor(IdentityUserManager userManager, IdentityRoleManager roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private static readonly List<DemoUser> DemoUsers = new()
    {
        new("admin@erpplatform.com", "System", "Admin", "Admin123!", "+201000000000"),
        new("ahmed.hamdi@erpplatform.com", "Ahmed", "Hamdi", "Admin123!", "+201011111111"),
        new("sara.mansour@erpplatform.com", "Sara", "Mansour", "Manager123!", "+201022222222"),
        new("omar.khaled@erpplatform.com", "Omar", "Khaled", "Employee123!", "+201033333333"),
        new("lina.nasser@erpplatform.com", "Lina", "Nasser", "Staff123!", "+201044444444")
    };

    /// <summary>
    /// Role each demo user is placed in. The demo "Admin" account gets the
    /// built-in admin role so it can actually manage the RBAC module. The
    /// others get realistic non-admin roles the admin can grant permissions to.
    /// </summary>
    private static readonly Dictionary<string, string> DemoRoleByEmail = new()
    {
        ["admin@erpplatform.com"] = "admin",
        ["ahmed.hamdi@erpplatform.com"] = "admin",
        ["sara.mansour@erpplatform.com"] = "HR Manager",
        ["omar.khaled@erpplatform.com"] = "Employee",
        ["lina.nasser@erpplatform.com"] = "Employee"
    };

    public async Task SeedAsync(DataSeedContext context)
    {
        // The admin account must be in the admin role for the RBAC module to work.
        await EnsureInRoleAsync("admin@abp.io", "admin");

        foreach (var demo in DemoUsers)
        {
            var user = await _userManager.FindByEmailAsync(demo.Email);
            if (user == null)
            {
                user = new IdentityUser(Guid.NewGuid(), demo.Email, demo.Email)
                {
                    Name = demo.FirstName,
                    Surname = demo.LastName
                };

                var result = await _userManager.CreateAsync(user, demo.Password);
                if (!result.Succeeded)
                {
                    // Surface the reason during seeding instead of failing silently.
                    throw new AbpException(
                        $"Failed to seed demo user '{demo.Email}': " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!string.IsNullOrEmpty(demo.PhoneNumber) && user.PhoneNumber != demo.PhoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(user, demo.PhoneNumber);
                user.SetPhoneNumberConfirmed(true);
                await _userManager.UpdateAsync(user);
            }

            if (DemoRoleByEmail.TryGetValue(demo.Email, out var roleName))
            {
                await EnsureInRoleAsync(demo.Email, roleName);
            }
        }
    }

    /// <summary>
    /// Creates the role if it does not exist and adds the user to it. Safe to call
    /// repeatedly; both checks are idempotent.
    /// </summary>
    private async Task EnsureInRoleAsync(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return;
        }

        if (await _roleManager.FindByNameAsync(roleName) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), roleName));
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }

    private sealed record DemoUser(string Email, string FirstName, string LastName, string Password, string PhoneNumber);
}
