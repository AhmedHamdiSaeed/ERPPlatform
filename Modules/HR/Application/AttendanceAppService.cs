using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class AttendanceDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CheckIn { get; set; } = string.Empty;
        public string CheckOut { get; set; } = string.Empty;
        public decimal WorkingHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Status { get; set; } = "Present";
    }

    public class AttendanceAppService : CrudAppService<Attendance, AttendanceDto, Guid, PagedAndSortedResultRequestDto, AttendanceDto>
    {
        public AttendanceAppService(IRepository<Attendance, Guid> repository) : base(repository) { }

        public async Task<AttendanceDto> CheckInAsync(Guid employeeId, string employeeName, string departmentName)
        {
            var entity = new Attendance
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                DepartmentName = departmentName,
                Date = DateTime.UtcNow.Date,
                CheckIn = DateTime.UtcNow.ToString("hh:mm tt"),
                Status = "Present"
            };
            entity = await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        public async Task CheckOutAsync(Guid attendanceId)
        {
            var entity = await Repository.GetAsync(attendanceId);
            entity.CheckOut = DateTime.UtcNow.ToString("hh:mm tt");
            entity.WorkingHours = 8.0m;
            await Repository.UpdateAsync(entity);
        }
    }
}
