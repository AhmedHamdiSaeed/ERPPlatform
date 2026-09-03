using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace ERPPlatform.Application.RoleScopes;

public class RolePageScopeDto : EntityDto<Guid>
{
    public string RoleName { get; set; } = string.Empty;
    public string PageKey { get; set; } = string.Empty;
    public DataScopeType ScopeType { get; set; } = DataScopeType.All;
    public List<Guid> TargetIds { get; set; } = new();
}

public class SaveRolePageScopeDto
{
    public string PageKey { get; set; } = string.Empty;
    public DataScopeType ScopeType { get; set; } = DataScopeType.All;
    public List<Guid> TargetIds { get; set; } = new();
}

public class SaveRolePageScopesInput
{
    public string RoleName { get; set; } = string.Empty;
    public List<SaveRolePageScopeDto> Scopes { get; set; } = new();
}

public interface IRolePageScopeAppService : IApplicationService
{
    Task<ListResultDto<RolePageScopeDto>> GetListAsync(string roleName);
    Task<ListResultDto<RolePageScopeDto>> SaveAsync(SaveRolePageScopesInput input);
}

/// <summary>
/// Stores the data scope (row-level visibility) configured per role per page.
/// Kept separate from ABP permissions: permissions gate the action, scope gates the rows.
/// </summary>
public class RolePageScopeAppService : ApplicationService, IRolePageScopeAppService
{
    private readonly IRepository<RolePageScope, Guid> _repository;

    public RolePageScopeAppService(IRepository<RolePageScope, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ListResultDto<RolePageScopeDto>> GetListAsync(string roleName)
    {
        var list = await _repository.GetListAsync(x => x.RoleName == roleName);

        var dtos = list.Select(x => new RolePageScopeDto
        {
            Id = x.Id,
            RoleName = x.RoleName,
            PageKey = x.PageKey,
            ScopeType = x.ScopeType,
            TargetIds = x.GetTargetIds()
        }).ToList();

        return new ListResultDto<RolePageScopeDto>(dtos);
    }

    public async Task<ListResultDto<RolePageScopeDto>> SaveAsync(SaveRolePageScopesInput input)
    {
        if (string.IsNullOrWhiteSpace(input.RoleName))
        {
            throw new UserFriendlyException("Role name is required.");
        }

        var existing = await _repository.GetListAsync(x => x.RoleName == input.RoleName);
        var byPageKey = existing.ToDictionary(x => x.PageKey);

        foreach (var item in input.Scopes)
        {
            if (string.IsNullOrWhiteSpace(item.PageKey))
            {
                continue;
            }

            // "All" is the default, so we drop the row instead of storing a no-op.
            if (item.ScopeType == DataScopeType.All)
            {
                if (byPageKey.TryGetValue(item.PageKey, out var unrestricted))
                {
                    await _repository.DeleteAsync(unrestricted);
                }

                continue;
            }

            if (byPageKey.TryGetValue(item.PageKey, out var scope))
            {
                scope.ScopeType = item.ScopeType;
                scope.SetTargetIds(item.TargetIds);
                await _repository.UpdateAsync(scope);
            }
            else
            {
                var created = new RolePageScope
                {
                    RoleName = input.RoleName,
                    PageKey = item.PageKey,
                    ScopeType = item.ScopeType
                };
                created.SetTargetIds(item.TargetIds);
                await _repository.InsertAsync(created);
            }
        }

        // Flush so the returned list reflects the inserts/updates we just made.
        if (CurrentUnitOfWork != null)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return await GetListAsync(input.RoleName);
    }
}
