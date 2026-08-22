using System;
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
    }

    public class LeaveRequestAppService : CrudAppService<LeaveRequest, LeaveRequestDto, Guid, PagedAndSortedResultRequestDto, LeaveRequestDto>, ILeaveRequestAppService
    {
        public LeaveRequestAppService(IRepository<LeaveRequest, Guid> repository) : base(repository)
        {
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
