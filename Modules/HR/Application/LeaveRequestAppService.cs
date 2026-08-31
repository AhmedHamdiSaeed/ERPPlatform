using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class LeaveRequestDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = "Annual";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime AppliedDate { get; set; }
    }

public interface ILeaveRequestAppService : ICrudAppService<LeaveRequestDto, Guid, PagedAndSortedResultRequestDto, LeaveRequestDto>
{
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);
    Task<PagedResultDto<LeaveRequestDto>> GetListFilteredAsync(LeaveRequestGetListInput input);
}

public class LeaveRequestGetListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; } = string.Empty; // Search by employee name
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public string LeaveType { get; set; } = string.Empty; // Annual, Sick, Casual, etc.
    public Guid? EmployeeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class LeaveRequestAppService : CrudAppService<LeaveRequest, LeaveRequestDto, Guid, PagedAndSortedResultRequestDto, LeaveRequestDto>, ILeaveRequestAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public LeaveRequestAppService(
            IRepository<LeaveRequest, Guid> repository,
            IRepository<Employee, Guid> employeeRepository) : base(repository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task ApproveAsync(Guid id)
        {
            var leave = await Repository.GetAsync(id);
            if (leave.Status != "Pending")
            {
                throw new InvalidOperationException($"Only pending leave requests can be approved. Current status: {leave.Status}.");
            }
            leave.Status = "Approved";
            await Repository.UpdateAsync(leave);

            // Decrement leave balance on the employee
            var employees = await _employeeRepository.GetListAsync();
            var emp = employees.FirstOrDefault(e => e.Id == leave.EmployeeId);
            if (emp != null)
            {
                emp.LeaveBalance -= leave.DaysCount;
                if (emp.LeaveBalance < 0) emp.LeaveBalance = 0;
                await _employeeRepository.UpdateAsync(emp);
            }
        }

    public async Task RejectAsync(Guid id)
    {
        var leave = await Repository.GetAsync(id);
        if (leave.Status != "Pending")
        {
            throw new InvalidOperationException($"Only pending leave requests can be rejected. Current status: {leave.Status}.");
        }
        leave.Status = "Rejected";
        await Repository.UpdateAsync(leave);
    }

    public async Task<PagedResultDto<LeaveRequestDto>> GetListFilteredAsync(LeaveRequestGetListInput input)
    {
        var all = await Repository.GetListAsync();
        var query = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.ToLowerInvariant();
            query = query.Where(l => (l.EmployeeName ?? "").ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(l => l.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.LeaveType))
        {
            query = query.Where(l => l.LeaveType == input.LeaveType);
        }

        if (input.EmployeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == input.EmployeeId.Value);
        }

        if (input.DateFrom.HasValue)
        {
            query = query.Where(l => l.StartDate >= input.DateFrom.Value);
        }

        if (input.DateTo.HasValue)
        {
            query = query.Where(l => l.EndDate <= input.DateTo.Value);
        }

        query = query.OrderByDescending(l => l.CreationTime);

        var totalCount = query.Count();
        var items = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var dtos = items.Select(l => new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.EmployeeName,
            LeaveType = l.LeaveType,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            DaysCount = l.DaysCount,
            Reason = l.Reason,
            Status = l.Status,
            AppliedDate = l.CreationTime
        }).ToList();

        return new PagedResultDto<LeaveRequestDto>(totalCount, dtos);
    }

        protected override Task<LeaveRequest> MapToEntityAsync(LeaveRequestDto createInput)
        {
            return Task.FromResult(new LeaveRequest
            {
                EmployeeId = createInput.EmployeeId,
                EmployeeName = createInput.EmployeeName,
                LeaveType = string.IsNullOrWhiteSpace(createInput.LeaveType) ? "Annual" : createInput.LeaveType,
                StartDate = createInput.StartDate,
                EndDate = createInput.EndDate,
                DaysCount = createInput.DaysCount,
                Reason = createInput.Reason,
                Status = "Pending"
            });
        }

        protected override Task MapToEntityAsync(LeaveRequestDto updateInput, LeaveRequest entity)
        {
            entity.EmployeeId = updateInput.EmployeeId;
            entity.EmployeeName = updateInput.EmployeeName;
            entity.LeaveType = updateInput.LeaveType;
            entity.StartDate = updateInput.StartDate;
            entity.EndDate = updateInput.EndDate;
            entity.DaysCount = updateInput.DaysCount;
            entity.Reason = updateInput.Reason;
            return Task.CompletedTask;
        }

        protected override Task<LeaveRequestDto> MapToGetOutputDtoAsync(LeaveRequest entity)
        {
            return Task.FromResult(new LeaveRequestDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = entity.EmployeeName,
                LeaveType = entity.LeaveType,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                DaysCount = entity.DaysCount,
                Reason = entity.Reason,
                Status = entity.Status,
                AppliedDate = entity.CreationTime
            });
        }
    }
}
