using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Application.RoleScopes;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class EmployeeDto : EntityDto<Guid>
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
        public decimal LeaveBalance { get; set; } = 21.0m;
    }

    public class CreateUpdateEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public decimal LeaveBalance { get; set; } = 21.0m;
    }

    public class EmployeeGetListInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } = string.Empty; // Search by name, email, phone, code
        public string Status { get; set; } = string.Empty; // Active, Inactive, Terminated
        public string DepartmentName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime? JoiningDateFrom { get; set; }
        public DateTime? JoiningDateTo { get; set; }
    }

    public interface IEmployeeAppService : ICrudAppService<EmployeeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDto>
    {
        Task<decimal> GetLeaveBalanceAsync(Guid id);
        Task<PagedResultDto<EmployeeDto>> GetListFilteredAsync(EmployeeGetListInput input);
    }

    public class EmployeeAppService : CrudAppService<Employee, EmployeeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDto>, IEmployeeAppService
    {
        private readonly IDataScopeService _dataScopeService;

        public EmployeeAppService(IRepository<Employee, Guid> repository, IDataScopeService dataScopeService) : base(repository)
        {
            _dataScopeService = dataScopeService;
        }

        public async Task<decimal> GetLeaveBalanceAsync(Guid id)
        {
            var emp = await Repository.GetAsync(id);
            return emp.LeaveBalance;
        }

        public async Task<PagedResultDto<EmployeeDto>> GetListFilteredAsync(EmployeeGetListInput input)
        {
            var all = await Repository.GetListAsync();
            var query = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var filter = input.Filter.ToLowerInvariant();
                query = query.Where(e =>
                    (e.Name ?? "").ToLower().Contains(filter) ||
                    (e.Email ?? "").ToLower().Contains(filter) ||
                    (e.Phone ?? "").ToLower().Contains(filter) ||
                    (e.EmployeeCode ?? "").ToLower().Contains(filter));
            }

            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                query = query.Where(e => e.Status == input.Status);
            }

            if (!string.IsNullOrWhiteSpace(input.DepartmentName))
            {
                query = query.Where(e => e.DepartmentName == input.DepartmentName);
            }

            if (!string.IsNullOrWhiteSpace(input.Position))
            {
                query = query.Where(e => e.Position == input.Position);
            }

            if (input.JoiningDateFrom.HasValue)
            {
                query = query.Where(e => e.JoiningDate >= input.JoiningDateFrom.Value);
            }

            if (input.JoiningDateTo.HasValue)
            {
                query = query.Where(e => e.JoiningDate <= input.JoiningDateTo.Value);
            }

            // Row-level scoping: keep only employees visible under this user's role scopes.
            var scope = await _dataScopeService.GetFilterAsync(DataScopePageKeys.Employees);
            if (scope.IsRestricted)
            {
                query = query.Where(e =>
                    (scope.EmployeeIds.Count > 0 && scope.EmployeeIds.Contains(e.Id)) ||
                    (scope.DepartmentIds.Count > 0 && e.DepartmentId.HasValue && scope.DepartmentIds.Contains(e.DepartmentId.Value)) ||
                    (scope.BranchIds.Count > 0 && e.BranchId.HasValue && scope.BranchIds.Contains(e.BranchId.Value)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                query = input.Sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(e => e.Name)
                    : query.OrderBy(e => e.Name);
            }
            else
            {
                query = query.OrderBy(e => e.Name);
            }

            var totalCount = query.Count();
            var items = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var dtos = items.Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Position = e.Position,
                DepartmentName = e.DepartmentName,
                Salary = e.Salary,
                JoiningDate = e.JoiningDate,
                Status = e.Status,
                Avatar = e.Avatar,
                ManagerName = e.ManagerName,
                Location = e.Location,
                LeaveBalance = e.LeaveBalance
            }).ToList();

            return new PagedResultDto<EmployeeDto>(totalCount, dtos);
        }

        protected override Task<Employee> MapToEntityAsync(CreateUpdateEmployeeDto createInput)
        {
            return Task.FromResult(new Employee
            {
                EmployeeCode = createInput.EmployeeCode,
                Name = createInput.Name,
                Email = createInput.Email,
                Phone = createInput.Phone,
                Position = createInput.Position,
                DepartmentName = createInput.DepartmentName,
                Salary = createInput.Salary,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Active" : createInput.Status,
                Location = "Cairo HQ",
                JoiningDate = createInput.JoiningDate != default ? createInput.JoiningDate : DateTime.UtcNow,
                LeaveBalance = createInput.LeaveBalance > 0 ? createInput.LeaveBalance : 21.0m
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateEmployeeDto updateInput, Employee entity)
        {
            entity.EmployeeCode = updateInput.EmployeeCode;
            entity.Name = updateInput.Name;
            entity.Email = updateInput.Email;
            entity.Phone = updateInput.Phone;
            entity.Position = updateInput.Position;
            entity.DepartmentName = updateInput.DepartmentName;
            entity.Salary = updateInput.Salary;
            if (!string.IsNullOrWhiteSpace(updateInput.Status))
            {
                entity.Status = updateInput.Status;
            }
            entity.LeaveBalance = updateInput.LeaveBalance;
            return Task.CompletedTask;
        }

        protected override Task<EmployeeDto> MapToGetOutputDtoAsync(Employee entity)
        {
            return Task.FromResult(new EmployeeDto
            {
                Id = entity.Id,
                EmployeeCode = entity.EmployeeCode,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Position = entity.Position,
                DepartmentName = entity.DepartmentName,
                Salary = entity.Salary,
                JoiningDate = entity.JoiningDate,
                Status = entity.Status,
                Avatar = entity.Avatar,
                ManagerName = entity.ManagerName,
                Location = entity.Location,
                LeaveBalance = entity.LeaveBalance
            });
        }
    }
}
