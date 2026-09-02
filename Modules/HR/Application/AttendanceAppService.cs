using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    /// <summary>
    /// Get-list input for attendance. <see cref="EmployeeId"/> is optional: when supplied the list is
    /// restricted to a single employee (used by the employee detail page).
    /// </summary>
    public class AttendanceGetListInput : PagedAndSortedResultRequestDto
    {
        public Guid? EmployeeId { get; set; }
    }

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
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
    }

    public class AttendanceAppService : CrudAppService<Attendance, AttendanceDto, Guid, AttendanceGetListInput, AttendanceDto>
    {
        public AttendanceAppService(IRepository<Attendance, Guid> repository) : base(repository) { }

        protected override async Task<IQueryable<Attendance>> CreateFilteredQueryAsync(AttendanceGetListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (input.EmployeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == input.EmployeeId.Value);
            }

            return query;
        }

        public async Task<AttendanceDto> CheckInAsync(Guid employeeId, string employeeName, string departmentName, double? latitude = null, double? longitude = null)
        {
            var entity = new Attendance
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                DepartmentName = departmentName,
                Date = DateTime.UtcNow.Date,
                CheckIn = DateTime.UtcNow.ToString("hh:mm tt"),
                Status = "Present",
                CheckInLatitude = latitude,
                CheckInLongitude = longitude
            };
            entity = await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        public async Task CheckOutAsync(Guid attendanceId, double? latitude = null, double? longitude = null)
        {
            var entity = await Repository.GetAsync(attendanceId);
            entity.CheckOut = DateTime.UtcNow.ToString("hh:mm tt");
            entity.WorkingHours = 8.0m;
            entity.CheckOutLatitude = latitude;
            entity.CheckOutLongitude = longitude;
            await Repository.UpdateAsync(entity);
        }

        protected override Task<AttendanceDto> MapToGetOutputDtoAsync(Attendance entity)
        {
            return Task.FromResult(new AttendanceDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = entity.EmployeeName,
                DepartmentName = entity.DepartmentName,
                Date = entity.Date,
                CheckIn = entity.CheckIn,
                CheckOut = entity.CheckOut,
                WorkingHours = entity.WorkingHours,
                OvertimeHours = entity.OvertimeHours,
                Status = entity.Status,
                CheckInLatitude = entity.CheckInLatitude,
                CheckInLongitude = entity.CheckInLongitude,
                CheckOutLatitude = entity.CheckOutLatitude,
                CheckOutLongitude = entity.CheckOutLongitude
            });
        }
    }
}
