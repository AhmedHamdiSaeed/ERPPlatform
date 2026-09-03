using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace ERPPlatform.Application.RoleScopes;

/// <summary>
/// Concrete row filters produced by resolving a page's configured scope for the current user.
/// A page is unrestricted when IsRestricted is false. When restricted, the allowed rows are the
/// UNION of the id sets (a user with several roles gets the widest access any role grants).
/// </summary>
public class DataScopeFilter
{
    public bool IsRestricted { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
    public List<Guid> BranchIds { get; set; } = new();
    public List<Guid> EmployeeIds { get; set; } = new();
}

public interface IDataScopeService : ITransientDependency
{
    /// <summary>Resolves the effective data scope for the current user on the given page.</summary>
    Task<DataScopeFilter> GetFilterAsync(string pageKey);
}

public class DataScopeService : IDataScopeService, ITransientDependency
{
    private readonly IRepository<RolePageScope, Guid> _scopeRepository;
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly IRepository<UserBranchAssignment, Guid> _userBranchAssignmentRepository;
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentUser _currentUser;

    public DataScopeService(
        IRepository<RolePageScope, Guid> scopeRepository,
        IRepository<Employee, Guid> employeeRepository,
        IRepository<UserBranchAssignment, Guid> userBranchAssignmentRepository,
        IdentityUserManager userManager,
        ICurrentUser currentUser)
    {
        _scopeRepository = scopeRepository;
        _employeeRepository = employeeRepository;
        _userBranchAssignmentRepository = userBranchAssignmentRepository;
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<DataScopeFilter> GetFilterAsync(string pageKey)
    {
        var filter = new DataScopeFilter();

        if (string.IsNullOrWhiteSpace(pageKey) || !_currentUser.IsAuthenticated)
        {
            return filter;
        }

        var roleNames = await GetCurrentUserRoleNamesAsync();
        if (roleNames.Count == 0)
        {
            return filter;
        }

        var scopes = await _scopeRepository.GetListAsync(
            x => x.PageKey == pageKey && roleNames.Contains(x.RoleName));

        if (scopes.Count == 0)
        {
            return filter;
        }

        // Any role granting full access wins over more restrictive roles.
        if (scopes.Any(s => s.ScopeType == DataScopeType.All))
        {
            return filter;
        }

        Employee? currentEmployee = null;

        foreach (var scope in scopes)
        {
            switch (scope.ScopeType)
            {
                case DataScopeType.MyDepartment:
                    currentEmployee ??= await FindCurrentEmployeeAsync();
                    if (currentEmployee?.DepartmentId is { } departmentId)
                    {
                        filter.DepartmentIds.Add(departmentId);
                    }

                    filter.IsRestricted = true;
                    break;

                case DataScopeType.MyBranch:
                    currentEmployee ??= await FindCurrentEmployeeAsync();
                    var branchId = currentEmployee?.BranchId ?? await GetDefaultBranchIdAsync();
                    if (branchId.HasValue)
                    {
                        filter.BranchIds.Add(branchId.Value);
                    }

                    filter.IsRestricted = true;
                    break;

                case DataScopeType.SpecificBranch:
                    filter.BranchIds.AddRange(scope.GetTargetIds());
                    filter.IsRestricted = true;
                    break;

                case DataScopeType.SpecificDepartment:
                    filter.DepartmentIds.AddRange(scope.GetTargetIds());
                    filter.IsRestricted = true;
                    break;

                case DataScopeType.SpecificEmployees:
                    filter.EmployeeIds.AddRange(scope.GetTargetIds());
                    filter.IsRestricted = true;
                    break;
            }
        }

        filter.DepartmentIds = filter.DepartmentIds.Distinct().ToList();
        filter.BranchIds = filter.BranchIds.Distinct().ToList();
        filter.EmployeeIds = filter.EmployeeIds.Distinct().ToList();

        return filter;
    }

    private async Task<List<string>> GetCurrentUserRoleNamesAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return new List<string>();
        }

        var user = await _userManager.FindByIdAsync(_currentUser.Id.Value.ToString());
        if (user == null)
        {
            return new List<string>();
        }

        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    /// <summary>
    /// Links the signed-in IdentityUser to an Employee record. The domain has no UserId on
    /// Employee, so email is the practical join key.
    /// </summary>
    private async Task<Employee?> FindCurrentEmployeeAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentUser.Email))
        {
            return null;
        }

        return await _employeeRepository.FirstOrDefaultAsync(x => x.Email == _currentUser.Email);
    }

    private async Task<Guid?> GetDefaultBranchIdAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return null;
        }

        var assignments = await _userBranchAssignmentRepository.GetListAsync(x => x.UserId == _currentUser.Id.Value);
        if (assignments.Count == 0)
        {
            return null;
        }

        var preferred = assignments.FirstOrDefault(x => x.IsDefault) ?? assignments.First();
        return preferred.BranchId;
    }
}
