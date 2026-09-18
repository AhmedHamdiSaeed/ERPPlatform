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
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
        public decimal LeaveBalance { get; set; } = 21.0m;

        // Master Data Enrichment
        public string NationalId { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = "Egyptian";
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Male";
        public string MaritalStatus { get; set; } = "Single";
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmergencyContactRelation { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = "FullTime";
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string CostCenterCode { get; set; } = string.Empty;
        public int NoticePeriodDays { get; set; } = 30;
    }

    public class CreateUpdateEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
        public decimal LeaveBalance { get; set; } = 21.0m;

        // Master Data Fields
        public string NationalId { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = "Egyptian";
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Male";
        public string MaritalStatus { get; set; } = "Single";
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmergencyContactRelation { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = "FullTime";
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string CostCenterCode { get; set; } = string.Empty;
        public int NoticePeriodDays { get; set; } = 30;
    }

    public class EmployeeGetListInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } = string.Empty; // Search by name, email, phone, code
        public string Status { get; set; } = string.Empty; // Active, Inactive, Terminated
        public string DepartmentName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
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
                    (e.EmployeeCode ?? "").ToLower().Contains(filter) ||
                    (e.NationalId ?? "").ToLower().Contains(filter));
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

            if (!string.IsNullOrWhiteSpace(input.EmploymentType))
            {
                query = query.Where(e => e.EmploymentType == input.EmploymentType);
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

            var dtos = items.Select(e => MapEntityToDto(e)).ToList();

            return new PagedResultDto<EmployeeDto>(totalCount, dtos);
        }

        private static EmployeeDto MapEntityToDto(Employee entity)
        {
            return new EmployeeDto
            {
                Id = entity.Id,
                EmployeeCode = entity.EmployeeCode,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Position = entity.Position,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.DepartmentName,
                BranchId = entity.BranchId,
                BranchName = entity.BranchName,
                Salary = entity.Salary,
                JoiningDate = entity.JoiningDate,
                Status = entity.Status,
                Avatar = entity.Avatar,
                ManagerName = entity.ManagerName,
                Location = entity.Location,
                LeaveBalance = entity.LeaveBalance,
                NationalId = entity.NationalId,
                PassportNumber = entity.PassportNumber,
                Nationality = entity.Nationality,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                MaritalStatus = entity.MaritalStatus,
                EmergencyContactName = entity.EmergencyContactName,
                EmergencyContactPhone = entity.EmergencyContactPhone,
                EmergencyContactRelation = entity.EmergencyContactRelation,
                BankName = entity.BankName,
                BankAccountNumber = entity.BankAccountNumber,
                Iban = entity.Iban,
                SwiftCode = entity.SwiftCode,
                EmploymentType = entity.EmploymentType,
                ProbationEndDate = entity.ProbationEndDate,
                ContractEndDate = entity.ContractEndDate,
                JobGradeId = entity.JobGradeId,
                JobGradeName = entity.JobGradeName,
                CostCenterCode = entity.CostCenterCode,
                NoticePeriodDays = entity.NoticePeriodDays
            };
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
                DepartmentId = createInput.DepartmentId,
                DepartmentName = createInput.DepartmentName,
                BranchId = createInput.BranchId,
                BranchName = createInput.BranchName,
                Salary = createInput.Salary,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Active" : createInput.Status,
                Location = string.IsNullOrWhiteSpace(createInput.Location) ? "Cairo HQ" : createInput.Location,
                JoiningDate = createInput.JoiningDate != default ? createInput.JoiningDate : DateTime.UtcNow,
                LeaveBalance = createInput.LeaveBalance > 0 ? createInput.LeaveBalance : 21.0m,
                Avatar = createInput.Avatar,
                ManagerName = createInput.ManagerName,
                NationalId = createInput.NationalId,
                PassportNumber = createInput.PassportNumber,
                Nationality = string.IsNullOrWhiteSpace(createInput.Nationality) ? "Egyptian" : createInput.Nationality,
                DateOfBirth = createInput.DateOfBirth,
                Gender = string.IsNullOrWhiteSpace(createInput.Gender) ? "Male" : createInput.Gender,
                MaritalStatus = string.IsNullOrWhiteSpace(createInput.MaritalStatus) ? "Single" : createInput.MaritalStatus,
                EmergencyContactName = createInput.EmergencyContactName,
                EmergencyContactPhone = createInput.EmergencyContactPhone,
                EmergencyContactRelation = createInput.EmergencyContactRelation,
                BankName = createInput.BankName,
                BankAccountNumber = createInput.BankAccountNumber,
                Iban = createInput.Iban,
                SwiftCode = createInput.SwiftCode,
                EmploymentType = string.IsNullOrWhiteSpace(createInput.EmploymentType) ? "FullTime" : createInput.EmploymentType,
                ProbationEndDate = createInput.ProbationEndDate,
                ContractEndDate = createInput.ContractEndDate,
                JobGradeId = createInput.JobGradeId,
                JobGradeName = createInput.JobGradeName,
                CostCenterCode = createInput.CostCenterCode,
                NoticePeriodDays = createInput.NoticePeriodDays > 0 ? createInput.NoticePeriodDays : 30
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateEmployeeDto updateInput, Employee entity)
        {
            entity.EmployeeCode = updateInput.EmployeeCode;
            entity.Name = updateInput.Name;
            entity.Email = updateInput.Email;
            entity.Phone = updateInput.Phone;
            entity.Position = updateInput.Position;
            entity.DepartmentId = updateInput.DepartmentId;
            entity.DepartmentName = updateInput.DepartmentName;
            entity.BranchId = updateInput.BranchId;
            entity.BranchName = updateInput.BranchName;
            entity.Salary = updateInput.Salary;
            if (!string.IsNullOrWhiteSpace(updateInput.Status))
            {
                entity.Status = updateInput.Status;
            }
            if (!string.IsNullOrWhiteSpace(updateInput.Location))
            {
                entity.Location = updateInput.Location;
            }
            if (updateInput.JoiningDate != default)
            {
                entity.JoiningDate = updateInput.JoiningDate;
            }
            entity.LeaveBalance = updateInput.LeaveBalance;
            entity.Avatar = updateInput.Avatar;
            entity.ManagerName = updateInput.ManagerName;
            entity.NationalId = updateInput.NationalId;
            entity.PassportNumber = updateInput.PassportNumber;
            entity.Nationality = updateInput.Nationality;
            entity.DateOfBirth = updateInput.DateOfBirth;
            entity.Gender = updateInput.Gender;
            entity.MaritalStatus = updateInput.MaritalStatus;
            entity.EmergencyContactName = updateInput.EmergencyContactName;
            entity.EmergencyContactPhone = updateInput.EmergencyContactPhone;
            entity.EmergencyContactRelation = updateInput.EmergencyContactRelation;
            entity.BankName = updateInput.BankName;
            entity.BankAccountNumber = updateInput.BankAccountNumber;
            entity.Iban = updateInput.Iban;
            entity.SwiftCode = updateInput.SwiftCode;
            entity.EmploymentType = updateInput.EmploymentType;
            entity.ProbationEndDate = updateInput.ProbationEndDate;
            entity.ContractEndDate = updateInput.ContractEndDate;
            entity.JobGradeId = updateInput.JobGradeId;
            entity.JobGradeName = updateInput.JobGradeName;
            entity.CostCenterCode = updateInput.CostCenterCode;
            entity.NoticePeriodDays = updateInput.NoticePeriodDays;
            return Task.CompletedTask;
        }

        public override async Task<PagedResultDto<EmployeeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            return await GetListFilteredAsync(new EmployeeGetListInput
            {
                SkipCount = input.SkipCount,
                MaxResultCount = input.MaxResultCount,
                Sorting = input.Sorting
            });
        }

        protected override Task<EmployeeDto> MapToGetOutputDtoAsync(Employee entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }

        protected override Task<EmployeeDto> MapToGetListOutputDtoAsync(Employee entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
