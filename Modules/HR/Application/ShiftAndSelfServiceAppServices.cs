using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class WorkShiftDto : EntityDto<Guid>
    {
        public string ShiftCode { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int BreakMinutes { get; set; } = 60;
        public bool IsNightShift { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateWorkShiftDto
    {
        public string ShiftCode { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int BreakMinutes { get; set; } = 60;
        public bool IsNightShift { get; set; }
    }

    public class ShiftAssignmentDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid WorkShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class AssignShiftDto
    {
        public Guid EmployeeId { get; set; }
        public Guid WorkShiftId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public interface IShiftAppService : IApplicationService
    {
        Task<List<WorkShiftDto>> GetShiftsAsync();
        Task<WorkShiftDto> CreateShiftAsync(CreateWorkShiftDto input);
        Task<List<ShiftAssignmentDto>> GetShiftAssignmentsAsync(Guid? employeeId = null);
        Task<ShiftAssignmentDto> AssignShiftAsync(AssignShiftDto input);
    }

    public class ShiftAppService : ApplicationService, IShiftAppService
    {
        private readonly IRepository<WorkShift, Guid> _shiftRepo;
        private readonly IRepository<ShiftAssignment, Guid> _assignmentRepo;
        private readonly IRepository<Employee, Guid> _employeeRepo;

        public ShiftAppService(
            IRepository<WorkShift, Guid> shiftRepo,
            IRepository<ShiftAssignment, Guid> assignmentRepo,
            IRepository<Employee, Guid> employeeRepo)
        {
            _shiftRepo = shiftRepo;
            _assignmentRepo = assignmentRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<WorkShiftDto>> GetShiftsAsync()
        {
            var shifts = await _shiftRepo.GetListAsync();
            return shifts.Select(s => new WorkShiftDto
            {
                Id = s.Id,
                ShiftCode = s.ShiftCode,
                ShiftName = s.ShiftName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BreakMinutes = s.BreakMinutes,
                IsNightShift = s.IsNightShift,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<WorkShiftDto> CreateShiftAsync(CreateWorkShiftDto input)
        {
            var shift = new WorkShift
            {
                ShiftCode = !string.IsNullOrWhiteSpace(input.ShiftCode) ? input.ShiftCode : $"SHF-{DateTime.UtcNow.Ticks % 10000:D4}",
                ShiftName = input.ShiftName,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                BreakMinutes = input.BreakMinutes,
                IsNightShift = input.IsNightShift,
                IsActive = true
            };

            await _shiftRepo.InsertAsync(shift);
            return new WorkShiftDto
            {
                Id = shift.Id,
                ShiftCode = shift.ShiftCode,
                ShiftName = shift.ShiftName,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                BreakMinutes = shift.BreakMinutes,
                IsNightShift = shift.IsNightShift,
                IsActive = shift.IsActive
            };
        }

        public async Task<List<ShiftAssignmentDto>> GetShiftAssignmentsAsync(Guid? employeeId = null)
        {
            var query = await _assignmentRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var list = query.OrderByDescending(a => a.StartDate).ToList();
            return list.Select(a => new ShiftAssignmentDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.EmployeeName,
                WorkShiftId = a.WorkShiftId,
                ShiftName = a.ShiftName,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Notes = a.Notes
            }).ToList();
        }

        public async Task<ShiftAssignmentDto> AssignShiftAsync(AssignShiftDto input)
        {
            var emp = await _employeeRepo.GetAsync(input.EmployeeId);
            var shift = await _shiftRepo.GetAsync(input.WorkShiftId);

            var assignment = new ShiftAssignment
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                WorkShiftId = shift.Id,
                ShiftName = shift.ShiftName,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Notes = input.Notes
            };

            await _assignmentRepo.InsertAsync(assignment);
            return new ShiftAssignmentDto
            {
                Id = assignment.Id,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = assignment.EmployeeName,
                WorkShiftId = assignment.WorkShiftId,
                ShiftName = assignment.ShiftName,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Notes = assignment.Notes
            };
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Manager Self-Service (MSS) & Employee Self-Service (ESS) Aggregated Views
    // ────────────────────────────────────────────────────────────────
    public class TeamSummaryDto
    {
        public int TotalDirectReports { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public int PendingLeaveApprovals { get; set; }
        public int PendingActionApprovals { get; set; }
        public List<EmployeeDto> TeamMembers { get; set; } = new();
    }

    public interface ISelfServiceAppService : IApplicationService
    {
        Task<TeamSummaryDto> GetManagerTeamSummaryAsync(string managerName);
    }

    public class SelfServiceAppService : ApplicationService, ISelfServiceAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepo;
        private readonly IRepository<LeaveRequest, Guid> _leaveRepo;
        private readonly IRepository<Attendance, Guid> _attendanceRepo;
        private readonly IRepository<HrAction, Guid> _actionRepo;

        public SelfServiceAppService(
            IRepository<Employee, Guid> employeeRepo,
            IRepository<LeaveRequest, Guid> leaveRepo,
            IRepository<Attendance, Guid> attendanceRepo,
            IRepository<HrAction, Guid> actionRepo)
        {
            _employeeRepo = employeeRepo;
            _leaveRepo = leaveRepo;
            _attendanceRepo = attendanceRepo;
            _actionRepo = actionRepo;
        }

        public async Task<TeamSummaryDto> GetManagerTeamSummaryAsync(string managerName)
        {
            var empQuery = await _employeeRepo.GetQueryableAsync();
            var teamMembers = empQuery
                .Where(e => e.ManagerName == managerName || string.IsNullOrEmpty(managerName))
                .ToList();

            var teamMemberIds = teamMembers.Select(t => t.Id).ToList();
            var today = DateTime.UtcNow.Date;

            var attendanceQuery = await _attendanceRepo.GetQueryableAsync();
            var presentCount = attendanceQuery
                .Where(a => teamMemberIds.Contains(a.EmployeeId) && a.Date >= today && a.Status == "Present")
                .Count();

            var leaveQuery = await _leaveRepo.GetQueryableAsync();
            var onLeaveCount = leaveQuery
                .Where(l => teamMemberIds.Contains(l.EmployeeId) && l.Status == "Approved" && l.StartDate <= today && l.EndDate >= today)
                .Count();

            var pendingLeaves = leaveQuery
                .Where(l => teamMemberIds.Contains(l.EmployeeId) && l.Status == "Pending")
                .Count();

            var actionQuery = await _actionRepo.GetQueryableAsync();
            var pendingActions = actionQuery
                .Where(a => teamMemberIds.Contains(a.EmployeeId) && a.Status == "Pending")
                .Count();

            return new TeamSummaryDto
            {
                TotalDirectReports = teamMembers.Count,
                PresentToday = presentCount,
                OnLeaveToday = onLeaveCount,
                PendingLeaveApprovals = pendingLeaves,
                PendingActionApprovals = pendingActions,
                TeamMembers = teamMembers.Select(e => new EmployeeDto
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
                }).ToList()
            };
        }
    }
}
