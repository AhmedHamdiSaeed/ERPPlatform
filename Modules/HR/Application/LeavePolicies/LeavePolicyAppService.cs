using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application.LeavePolicies
{
    public class LeavePolicyDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string LeaveType { get; set; } = "Annual";
        public decimal AnnualAccrualDays { get; set; } = 21.0m;
        public decimal MaxCarryForwardDays { get; set; } = 5.0m;
        public decimal MaxConsecutiveDays { get; set; } = 30.0m;
        public bool RequiresApproval { get; set; } = true;
        public bool AllowHalfDay { get; set; } = true;
        public int ProbationPeriodMonths { get; set; } = 3;
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateUpdateLeavePolicyDto
    {
        public string Name { get; set; } = string.Empty;
        public string LeaveType { get; set; } = "Annual";
        public decimal AnnualAccrualDays { get; set; } = 21.0m;
        public decimal MaxCarryForwardDays { get; set; } = 5.0m;
        public decimal MaxConsecutiveDays { get; set; } = 30.0m;
        public bool RequiresApproval { get; set; } = true;
        public bool AllowHalfDay { get; set; } = true;
        public int ProbationPeriodMonths { get; set; } = 3;
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }

    public class LeaveBalanceResultDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = "Annual";
        public decimal CurrentBalance { get; set; }
        public decimal AccruedThisYear { get; set; }
        public decimal UsedThisYear { get; set; }
        public decimal CarryForward { get; set; }
        public decimal MaxCarryForward { get; set; }
        public decimal AvailableAfterCarryForward { get; set; }
    }

    public interface ILeavePolicyAppService : ICrudAppService<LeavePolicyDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateLeavePolicyDto>
    {
        Task<LeavePolicyDto> GetByLeaveTypeAsync(string leaveType);
        Task<ListResultDto<LeaveBalanceResultDto>> GetLeaveBalancesAsync(Guid employeeId);
        Task AccrueLeaveAsync(Guid employeeId, string leaveType);
        Task CarryForwardAsync(Guid employeeId, string leaveType);
    }

    public class LeavePolicyAppService : CrudAppService<LeavePolicy, LeavePolicyDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateLeavePolicyDto>, ILeavePolicyAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<LeaveRequest, Guid> _leaveRequestRepository;

        public LeavePolicyAppService(
            IRepository<LeavePolicy, Guid> repository,
            IRepository<Employee, Guid> employeeRepository,
            IRepository<LeaveRequest, Guid> leaveRequestRepository) : base(repository)
        {
            _employeeRepository = employeeRepository;
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<LeavePolicyDto> GetByLeaveTypeAsync(string leaveType)
        {
            var policies = await Repository.GetListAsync();
            var policy = policies.FirstOrDefault(p =>
                p.LeaveType.Equals(leaveType, StringComparison.OrdinalIgnoreCase) && p.IsActive);

            if (policy == null)
            {
                throw new ArgumentException($"No active leave policy found for leave type '{leaveType}'.");
            }

            return await MapToGetOutputDtoAsync(policy);
        }

        public async Task<ListResultDto<LeaveBalanceResultDto>> GetLeaveBalancesAsync(Guid employeeId)
        {
            var employees = await _employeeRepository.GetListAsync();
            var employee = employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with ID '{employeeId}' not found.");
            }

            var leaveRequests = await _leaveRequestRepository.GetListAsync();
            var policies = await Repository.GetListAsync();

            var result = new List<LeaveBalanceResultDto>();
            var currentYear = DateTime.UtcNow.Year;

            // Get unique leave types from policies
            var leaveTypes = policies.Where(p => p.IsActive).Select(p => p.LeaveType).Distinct().ToList();

            foreach (var leaveType in leaveTypes)
            {
                var policy = policies.FirstOrDefault(p => p.LeaveType == leaveType && p.IsActive);
                if (policy == null) continue;

                var usedThisYear = leaveRequests
                    .Where(l => l.EmployeeId == employeeId &&
                                l.LeaveType == leaveType &&
                                l.Status == "Approved" &&
                                l.StartDate.Year == currentYear)
                    .Sum(l => l.DaysCount);

                var accrued = leaveType == "Annual" ? employee.LeaveBalance + usedThisYear : policy.AnnualAccrualDays;

                result.Add(new LeaveBalanceResultDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.Name,
                    LeaveType = leaveType,
                    CurrentBalance = leaveType == "Annual" ? employee.LeaveBalance : Math.Max(0, policy.AnnualAccrualDays - usedThisYear),
                    AccruedThisYear = accrued,
                    UsedThisYear = usedThisYear,
                    CarryForward = Math.Min(employee.LeaveBalance, policy.MaxCarryForwardDays),
                    MaxCarryForward = policy.MaxCarryForwardDays,
                    AvailableAfterCarryForward = Math.Min(employee.LeaveBalance, policy.MaxCarryForwardDays)
                });
            }

            return new ListResultDto<LeaveBalanceResultDto>(result);
        }

        public async Task AccrueLeaveAsync(Guid employeeId, string leaveType)
        {
            var employees = await _employeeRepository.GetListAsync();
            var employee = employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with ID '{employeeId}' not found.");
            }

            var policies = await Repository.GetListAsync();
            var policy = policies.FirstOrDefault(p =>
                p.LeaveType.Equals(leaveType, StringComparison.OrdinalIgnoreCase) && p.IsActive);

            if (policy == null)
            {
                throw new ArgumentException($"No active leave policy found for leave type '{leaveType}'.");
            }

            // Check probation period
            var monthsEmployed = (DateTime.UtcNow - employee.JoiningDate).TotalDays / 30.0;
            if (monthsEmployed < policy.ProbationPeriodMonths)
            {
                throw new InvalidOperationException(
                    $"Employee '{employee.Name}' is still in probation period " +
                    $"({monthsEmployed:F1} months out of {policy.ProbationPeriodMonths} required).");
            }

            // Accrue prorated annual amount
            var monthlyAccrual = policy.AnnualAccrualDays / 12.0m;
            if (leaveType == "Annual")
            {
                employee.LeaveBalance += monthlyAccrual;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        public async Task CarryForwardAsync(Guid employeeId, string leaveType)
        {
            var employees = await _employeeRepository.GetListAsync();
            var employee = employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with ID '{employeeId}' not found.");
            }

            var policies = await Repository.GetListAsync();
            var policy = policies.FirstOrDefault(p =>
                p.LeaveType.Equals(leaveType, StringComparison.OrdinalIgnoreCase) && p.IsActive);

            if (policy == null)
            {
                throw new ArgumentException($"No active leave policy found for leave type '{leaveType}'.");
            }

            if (leaveType == "Annual")
            {
                var carryForward = Math.Min(employee.LeaveBalance, policy.MaxCarryForwardDays);
                employee.LeaveBalance = carryForward; // Reset to carry-forward amount for new year
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        protected override Task<LeavePolicy> MapToEntityAsync(CreateUpdateLeavePolicyDto createInput)
        {
            return Task.FromResult(new LeavePolicy
            {
                Name = createInput.Name,
                LeaveType = string.IsNullOrWhiteSpace(createInput.LeaveType) ? "Annual" : createInput.LeaveType,
                AnnualAccrualDays = createInput.AnnualAccrualDays > 0 ? createInput.AnnualAccrualDays : 21.0m,
                MaxCarryForwardDays = createInput.MaxCarryForwardDays,
                MaxConsecutiveDays = createInput.MaxConsecutiveDays > 0 ? createInput.MaxConsecutiveDays : 30.0m,
                RequiresApproval = createInput.RequiresApproval,
                AllowHalfDay = createInput.AllowHalfDay,
                ProbationPeriodMonths = createInput.ProbationPeriodMonths > 0 ? createInput.ProbationPeriodMonths : 3,
                IsActive = createInput.IsActive,
                Description = createInput.Description
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateLeavePolicyDto updateInput, LeavePolicy entity)
        {
            entity.Name = updateInput.Name;
            entity.LeaveType = updateInput.LeaveType;
            entity.AnnualAccrualDays = updateInput.AnnualAccrualDays;
            entity.MaxCarryForwardDays = updateInput.MaxCarryForwardDays;
            entity.MaxConsecutiveDays = updateInput.MaxConsecutiveDays;
            entity.RequiresApproval = updateInput.RequiresApproval;
            entity.AllowHalfDay = updateInput.AllowHalfDay;
            entity.ProbationPeriodMonths = updateInput.ProbationPeriodMonths;
            entity.IsActive = updateInput.IsActive;
            entity.Description = updateInput.Description;
            return Task.CompletedTask;
        }

        protected override Task<LeavePolicyDto> MapToGetOutputDtoAsync(LeavePolicy entity)
        {
            return Task.FromResult(new LeavePolicyDto
            {
                Id = entity.Id,
                Name = entity.Name,
                LeaveType = entity.LeaveType,
                AnnualAccrualDays = entity.AnnualAccrualDays,
                MaxCarryForwardDays = entity.MaxCarryForwardDays,
                MaxConsecutiveDays = entity.MaxConsecutiveDays,
                RequiresApproval = entity.RequiresApproval,
                AllowHalfDay = entity.AllowHalfDay,
                ProbationPeriodMonths = entity.ProbationPeriodMonths,
                IsActive = entity.IsActive,
                Description = entity.Description
            });
        }
    }
}
