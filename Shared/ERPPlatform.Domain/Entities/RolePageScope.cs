using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Domain.Entities
{
    /// <summary>
    /// How much data a role can see on a given page. Stored per role + page.
    /// Complements ABP permissions: permissions gate the action (view/create/edit/delete),
    /// this gates the rows the action applies to.
    /// </summary>
    public enum DataScopeType
    {
        /// <summary>No row restriction - sees every employee.</summary>
        All = 0,

        /// <summary>Only employees in the same department as the current user.</summary>
        MyDepartment = 1,

        /// <summary>Only employees in the same branch as the current user.</summary>
        MyBranch = 2,

        /// <summary>Only employees in the explicitly listed branches.</summary>
        SpecificBranch = 3,

        /// <summary>Only employees in the explicitly listed departments.</summary>
        SpecificDepartment = 4,

        /// <summary>Only the explicitly listed employees.</summary>
        SpecificEmployees = 5
    }

    /// <summary>
    /// Persists the data scope configured for one role on one page.
    /// </summary>
    public class RolePageScope : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>ABP role name (matches IdentityRole.Name and the "R" permission provider key).</summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>Page identifier, e.g. "ERPPlatform.Employees".</summary>
        public string PageKey { get; set; } = string.Empty;

        public DataScopeType ScopeType { get; set; } = DataScopeType.All;

        /// <summary>
        /// JSON array of ids targeted by SpecificBranch / SpecificDepartment / SpecificEmployees.
        /// Empty for All / MyDepartment / MyBranch, which are resolved from the current user.
        /// </summary>
        public string TargetIdsJson { get; set; } = "[]";

        public List<Guid> GetTargetIds()
        {
            if (string.IsNullOrWhiteSpace(TargetIdsJson))
            {
                return new List<Guid>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(TargetIdsJson) ?? new List<Guid>();
            }
            catch (JsonException)
            {
                return new List<Guid>();
            }
        }

        public void SetTargetIds(IEnumerable<Guid> ids)
        {
            TargetIdsJson = JsonSerializer.Serialize((ids ?? Enumerable.Empty<Guid>()).ToList());
        }
    }
}
