using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class HrActionDto : EntityDto<Guid>
    {
        public string ActionCode { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Promotion";
        public DateTime EffectiveDate { get; set; }
        public string Status { get; set; } = "Approved";
        public string PreviousValuesJson { get; set; } = "{}";
        public string NewValuesJson { get; set; } = "{}";
        public string RequestedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }

    public class CreateHrActionRequestDto
    {
        public Guid EmployeeId { get; set; }
        public string ActionType { get; set; } = "Promotion"; // Hire, Promotion, Transfer, SalaryChange, DepartmentChange, ManagerChange, Suspension, Termination, Resignation
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public string RequestedBy { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        // Structured Target Changes
        public string? NewPosition { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public string? NewDepartmentName { get; set; }
        public decimal? NewSalary { get; set; }
        public string? NewManagerName { get; set; }
        public string? NewLocation { get; set; }
        public string? NewStatus { get; set; }
        public string? NewJobGradeName { get; set; }
    }

    public interface IHrActionAppService : ICrudAppService<HrActionDto, Guid, PagedAndSortedResultRequestDto, CreateHrActionRequestDto>
    {
        Task<List<HrActionDto>> GetActionsByEmployeeIdAsync(Guid employeeId);
        Task<HrActionDto> ApproveActionAsync(Guid id, string approvedBy);
        Task<HrActionDto> RejectActionAsync(Guid id, string rejectedBy, string reason);
    }

    public class HrActionAppService : CrudAppService<HrAction, HrActionDto, Guid, PagedAndSortedResultRequestDto, CreateHrActionRequestDto>, IHrActionAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public HrActionAppService(
            IRepository<HrAction, Guid> repository,
            IRepository<Employee, Guid> employeeRepository) : base(repository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<List<HrActionDto>> GetActionsByEmployeeIdAsync(Guid employeeId)
        {
            var list = await Repository.GetListAsync(a => a.EmployeeId == employeeId);
            return list.OrderByDescending(a => a.EffectiveDate).Select(MapEntityToDto).ToList();
        }

        public override async Task<HrActionDto> CreateAsync(CreateHrActionRequestDto input)
        {
            var employee = await _employeeRepository.GetAsync(input.EmployeeId);

            var prevValues = new Dictionary<string, object>
            {
                ["Position"] = employee.Position,
                ["DepartmentName"] = employee.DepartmentName,
                ["Salary"] = employee.Salary,
                ["ManagerName"] = employee.ManagerName,
                ["Location"] = employee.Location,
                ["Status"] = employee.Status,
                ["JobGradeName"] = employee.JobGradeName
            };

            var newValues = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(input.NewPosition)) newValues["Position"] = input.NewPosition;
            if (!string.IsNullOrWhiteSpace(input.NewDepartmentName)) newValues["DepartmentName"] = input.NewDepartmentName;
            if (input.NewSalary.HasValue) newValues["Salary"] = input.NewSalary.Value;
            if (!string.IsNullOrWhiteSpace(input.NewManagerName)) newValues["ManagerName"] = input.NewManagerName;
            if (!string.IsNullOrWhiteSpace(input.NewLocation)) newValues["Location"] = input.NewLocation;
            if (!string.IsNullOrWhiteSpace(input.NewStatus)) newValues["Status"] = input.NewStatus;
            if (!string.IsNullOrWhiteSpace(input.NewJobGradeName)) newValues["JobGradeName"] = input.NewJobGradeName;

            var actionCode = $"HRA-{DateTime.UtcNow:yyyyMM}-{new Random().Next(1000, 9999)}";
            var action = new HrAction
            {
                ActionCode = actionCode,
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                ActionType = input.ActionType,
                EffectiveDate = input.EffectiveDate,
                Status = "Approved", // Auto-approved or pending based on workflow
                PreviousValuesJson = JsonSerializer.Serialize(prevValues),
                NewValuesJson = JsonSerializer.Serialize(newValues),
                RequestedBy = string.IsNullOrWhiteSpace(input.RequestedBy) ? "HR Admin" : input.RequestedBy,
                ApprovedBy = "HR Admin",
                ApprovalDate = DateTime.UtcNow,
                Remarks = input.Remarks
            };

            await Repository.InsertAsync(action);

            // Apply updates to the employee directly
            ApplyChangesToEmployee(employee, input);
            await _employeeRepository.UpdateAsync(employee);

            return MapEntityToDto(action);
        }

        public async Task<HrActionDto> ApproveActionAsync(Guid id, string approvedBy)
        {
            var action = await Repository.GetAsync(id);
            action.Status = "Approved";
            action.ApprovedBy = approvedBy;
            action.ApprovalDate = DateTime.UtcNow;
            await Repository.UpdateAsync(action);

            var employee = await _employeeRepository.FindAsync(action.EmployeeId);
            if (employee != null && !string.IsNullOrWhiteSpace(action.NewValuesJson))
            {
                var doc = JsonDocument.Parse(action.NewValuesJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("Position", out var pos)) employee.Position = pos.GetString() ?? employee.Position;
                if (root.TryGetProperty("DepartmentName", out var dept)) employee.DepartmentName = dept.GetString() ?? employee.DepartmentName;
                if (root.TryGetProperty("Salary", out var sal)) employee.Salary = sal.GetDecimal();
                if (root.TryGetProperty("ManagerName", out var mgr)) employee.ManagerName = mgr.GetString() ?? employee.ManagerName;
                if (root.TryGetProperty("Location", out var loc)) employee.Location = loc.GetString() ?? employee.Location;
                if (root.TryGetProperty("Status", out var st)) employee.Status = st.GetString() ?? employee.Status;
                if (root.TryGetProperty("JobGradeName", out var gr)) employee.JobGradeName = gr.GetString() ?? employee.JobGradeName;

                await _employeeRepository.UpdateAsync(employee);
            }

            return MapEntityToDto(action);
        }

        public async Task<HrActionDto> RejectActionAsync(Guid id, string rejectedBy, string reason)
        {
            var action = await Repository.GetAsync(id);
            action.Status = "Rejected";
            action.ApprovedBy = rejectedBy;
            action.ApprovalDate = DateTime.UtcNow;
            action.Remarks = (action.Remarks + $" | Rejected: {reason}").TrimStart('|', ' ');
            await Repository.UpdateAsync(action);
            return MapEntityToDto(action);
        }

        private static void ApplyChangesToEmployee(Employee employee, CreateHrActionRequestDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.NewPosition)) employee.Position = input.NewPosition;
            if (!string.IsNullOrWhiteSpace(input.NewDepartmentName))
            {
                employee.DepartmentName = input.NewDepartmentName;
                if (input.NewDepartmentId.HasValue) employee.DepartmentId = input.NewDepartmentId.Value;
            }
            if (input.NewSalary.HasValue) employee.Salary = input.NewSalary.Value;
            if (!string.IsNullOrWhiteSpace(input.NewManagerName)) employee.ManagerName = input.NewManagerName;
            if (!string.IsNullOrWhiteSpace(input.NewLocation)) employee.Location = input.NewLocation;
            if (!string.IsNullOrWhiteSpace(input.NewStatus)) employee.Status = input.NewStatus;
            if (!string.IsNullOrWhiteSpace(input.NewJobGradeName)) employee.JobGradeName = input.NewJobGradeName;
        }

        private static HrActionDto MapEntityToDto(HrAction a)
        {
            return new HrActionDto
            {
                Id = a.Id,
                ActionCode = a.ActionCode,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.EmployeeName,
                ActionType = a.ActionType,
                EffectiveDate = a.EffectiveDate,
                Status = a.Status,
                PreviousValuesJson = a.PreviousValuesJson,
                NewValuesJson = a.NewValuesJson,
                RequestedBy = a.RequestedBy,
                ApprovedBy = a.ApprovedBy,
                ApprovalDate = a.ApprovalDate,
                Remarks = a.Remarks,
                CreationTime = a.CreationTime
            };
        }

        protected override Task<HrActionDto> MapToGetOutputDtoAsync(HrAction entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
