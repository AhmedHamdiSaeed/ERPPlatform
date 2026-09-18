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
    public class OnboardingTaskDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "IT";
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateOnboardingTaskDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "IT"; // IT, HR, Finance, Admin, Department
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string Notes { get; set; } = string.Empty;
    }

    public class OffboardingRequestDto : EntityDto<Guid>
    {
        public string RequestNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime ResignationDate { get; set; }
        public DateTime LastWorkingDay { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "ClearanceInProgress";
        public string ItClearanceStatus { get; set; } = "Pending";
        public string AdminClearanceStatus { get; set; } = "Pending";
        public string FinanceClearanceStatus { get; set; } = "Pending";
        public decimal OutstandingLoanBalance { get; set; }
        public decimal AccruedLeavePayout { get; set; }
        public decimal EndOfServiceGratuity { get; set; }
        public decimal NetFinalSettlement { get; set; }
        public string ExitInterviewNotes { get; set; } = string.Empty;
    }

    public class CreateOffboardingRequestDto
    {
        public Guid EmployeeId { get; set; }
        public DateTime ResignationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastWorkingDay { get; set; } = DateTime.UtcNow.AddDays(30);
        public string Reason { get; set; } = string.Empty;
        public string ExitInterviewNotes { get; set; } = string.Empty;
    }

    public interface ILifecycleOnboardingAppService
    {
        Task<List<OnboardingTaskDto>> GetTasksByEmployeeIdAsync(Guid employeeId);
        Task<OnboardingTaskDto> CreateTaskAsync(CreateOnboardingTaskDto input);
        Task<OnboardingTaskDto> CompleteTaskAsync(Guid taskId);
        Task<List<OnboardingTaskDto>> GenerateDefaultOnboardingChecklistAsync(Guid employeeId);

        Task<List<OffboardingRequestDto>> GetOffboardingRequestsAsync();
        Task<OffboardingRequestDto> SubmitOffboardingRequestAsync(CreateOffboardingRequestDto input);
        Task<OffboardingRequestDto> UpdateClearanceStatusAsync(Guid id, string department, string status);
        Task<OffboardingRequestDto> FinalizeOffboardingAsync(Guid id);
    }

    public class LifecycleOnboardingAppService : ApplicationService, ILifecycleOnboardingAppService
    {
        private readonly IRepository<OnboardingTask, Guid> _taskRepository;
        private readonly IRepository<OffboardingRequest, Guid> _offboardingRepository;
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<EmployeeLoan, Guid> _loanRepository;

        public LifecycleOnboardingAppService(
            IRepository<OnboardingTask, Guid> taskRepository,
            IRepository<OffboardingRequest, Guid> offboardingRepository,
            IRepository<Employee, Guid> employeeRepository,
            IRepository<EmployeeLoan, Guid> loanRepository)
        {
            _taskRepository = taskRepository;
            _offboardingRepository = offboardingRepository;
            _employeeRepository = employeeRepository;
            _loanRepository = loanRepository;
        }

        public async Task<List<OnboardingTaskDto>> GetTasksByEmployeeIdAsync(Guid employeeId)
        {
            var list = await _taskRepository.GetListAsync(t => t.EmployeeId == employeeId);
            return list.OrderBy(t => t.DueDate).Select(MapTaskToDto).ToList();
        }

        public async Task<OnboardingTaskDto> CreateTaskAsync(CreateOnboardingTaskDto input)
        {
            var task = new OnboardingTask
            {
                EmployeeId = input.EmployeeId,
                EmployeeName = input.EmployeeName,
                Title = input.Title,
                Category = input.Category,
                AssignedTo = input.AssignedTo,
                DueDate = input.DueDate,
                Status = "Pending",
                Notes = input.Notes
            };

            await _taskRepository.InsertAsync(task);
            return MapTaskToDto(task);
        }

        public async Task<OnboardingTaskDto> CompleteTaskAsync(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            task.Status = "Completed";
            task.CompletedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);
            return MapTaskToDto(task);
        }

        public async Task<List<OnboardingTaskDto>> GenerateDefaultOnboardingChecklistAsync(Guid employeeId)
        {
            var emp = await _employeeRepository.GetAsync(employeeId);
            var defaultTasks = new List<OnboardingTask>
            {
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Provision Company Email & System Accounts", Category = "IT", AssignedTo = "IT Support", DueDate = DateTime.UtcNow.AddDays(2), Status = "Pending" },
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Assign Laptop & Access Badge", Category = "IT", AssignedTo = "IT Support", DueDate = DateTime.UtcNow.AddDays(2), Status = "Pending" },
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Collect National ID & Educational Certificates", Category = "HR", AssignedTo = "HR Officer", DueDate = DateTime.UtcNow.AddDays(5), Status = "Pending" },
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Register Bank Account for Payroll", Category = "Finance", AssignedTo = "Payroll Accountant", DueDate = DateTime.UtcNow.AddDays(7), Status = "Pending" },
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Conduct HR Orientation & Policy Briefing", Category = "HR", AssignedTo = "HR Specialist", DueDate = DateTime.UtcNow.AddDays(3), Status = "Pending" },
                new() { EmployeeId = emp.Id, EmployeeName = emp.Name, Title = "Assign Department Mentor & Introduce Team", Category = "Department", AssignedTo = emp.ManagerName ?? "Department Head", DueDate = DateTime.UtcNow.AddDays(1), Status = "Pending" }
            };

            foreach (var t in defaultTasks)
            {
                await _taskRepository.InsertAsync(t);
            }

            return defaultTasks.Select(MapTaskToDto).ToList();
        }

        public async Task<List<OffboardingRequestDto>> GetOffboardingRequestsAsync()
        {
            var list = await _offboardingRepository.GetListAsync();
            return list.OrderByDescending(r => r.ResignationDate).Select(MapOffboardingToDto).ToList();
        }

        public async Task<OffboardingRequestDto> SubmitOffboardingRequestAsync(CreateOffboardingRequestDto input)
        {
            var emp = await _employeeRepository.GetAsync(input.EmployeeId);

            // Calculate outstanding loans
            var loans = await _loanRepository.GetListAsync(l => l.EmployeeId == emp.Id && l.Status == "Active");
            var outstandingLoan = loans.Sum(l => l.RemainingBalance);

            // Leave payout calculation (balance * daily wage)
            var dailyWage = emp.Salary > 0 ? (emp.Salary / 30m) : 0m;
            var leavePayout = (emp.LeaveBalance > 0 ? emp.LeaveBalance : 0m) * dailyWage;

            // Simple Gratuity calculation (half month per year for first 5 years)
            var tenureYears = (decimal)(DateTime.UtcNow - emp.JoiningDate).TotalDays / 365.25m;
            var gratuity = tenureYears >= 1 ? (tenureYears * 0.5m * emp.Salary) : 0m;

            var reqNum = $"OFF-{DateTime.UtcNow:yyyyMM}-{new Random().Next(100, 999)}";
            var offboarding = new OffboardingRequest
            {
                RequestNumber = reqNum,
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                DepartmentName = emp.DepartmentName,
                ResignationDate = input.ResignationDate,
                LastWorkingDay = input.LastWorkingDay,
                Reason = input.Reason,
                Status = "ClearanceInProgress",
                ItClearanceStatus = "Pending",
                AdminClearanceStatus = "Pending",
                FinanceClearanceStatus = "Pending",
                OutstandingLoanBalance = outstandingLoan,
                AccruedLeavePayout = leavePayout,
                EndOfServiceGratuity = gratuity,
                ExitInterviewNotes = input.ExitInterviewNotes
            };

            await _offboardingRepository.InsertAsync(offboarding);
            return MapOffboardingToDto(offboarding);
        }

        public async Task<OffboardingRequestDto> UpdateClearanceStatusAsync(Guid id, string department, string status)
        {
            var off = await _offboardingRepository.GetAsync(id);
            if (department.Equals("IT", StringComparison.OrdinalIgnoreCase)) off.ItClearanceStatus = status;
            if (department.Equals("Admin", StringComparison.OrdinalIgnoreCase)) off.AdminClearanceStatus = status;
            if (department.Equals("Finance", StringComparison.OrdinalIgnoreCase)) off.FinanceClearanceStatus = status;

            if (off.ItClearanceStatus == "Cleared" && off.AdminClearanceStatus == "Cleared" && off.FinanceClearanceStatus == "Cleared")
            {
                off.Status = "ReadyForFinalSettlement";
            }

            await _offboardingRepository.UpdateAsync(off);
            return MapOffboardingToDto(off);
        }

        public async Task<OffboardingRequestDto> FinalizeOffboardingAsync(Guid id)
        {
            var off = await _offboardingRepository.GetAsync(id);
            off.Status = "Finalized";
            await _offboardingRepository.UpdateAsync(off);

            var emp = await _employeeRepository.FindAsync(off.EmployeeId);
            if (emp != null)
            {
                emp.Status = "Terminated";
                await _employeeRepository.UpdateAsync(emp);
            }

            return MapOffboardingToDto(off);
        }

        private static OnboardingTaskDto MapTaskToDto(OnboardingTask t)
        {
            return new OnboardingTaskDto
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.EmployeeName,
                Title = t.Title,
                Category = t.Category,
                AssignedTo = t.AssignedTo,
                DueDate = t.DueDate,
                Status = t.Status,
                CompletedAt = t.CompletedAt,
                Notes = t.Notes
            };
        }

        private static OffboardingRequestDto MapOffboardingToDto(OffboardingRequest r)
        {
            return new OffboardingRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.EmployeeName,
                DepartmentName = r.DepartmentName,
                ResignationDate = r.ResignationDate,
                LastWorkingDay = r.LastWorkingDay,
                Reason = r.Reason,
                Status = r.Status,
                ItClearanceStatus = r.ItClearanceStatus,
                AdminClearanceStatus = r.AdminClearanceStatus,
                FinanceClearanceStatus = r.FinanceClearanceStatus,
                OutstandingLoanBalance = r.OutstandingLoanBalance,
                AccruedLeavePayout = r.AccruedLeavePayout,
                EndOfServiceGratuity = r.EndOfServiceGratuity,
                NetFinalSettlement = r.NetFinalSettlement,
                ExitInterviewNotes = r.ExitInterviewNotes
            };
        }
    }
}
