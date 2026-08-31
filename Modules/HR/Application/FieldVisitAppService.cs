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
    public class FieldVisitDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string Status { get; set; } = "Planned";
        public string NextFollowUpDate { get; set; } = string.Empty;
    }

    public class CreateFieldVisitDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; } = DateTime.UtcNow;
        public string Purpose { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class FieldVisitAppService : CrudAppService<FieldVisit, FieldVisitDto, Guid, PagedAndSortedResultRequestDto, CreateFieldVisitDto>
    {
        public FieldVisitAppService(IRepository<FieldVisit, Guid> repository) : base(repository) { }

        public async Task<FieldVisitDto> CheckInAsync(Guid id, double latitude, double longitude)
        {
            var fv = await Repository.GetAsync(id);
            fv.CheckInTime = DateTime.UtcNow;
            fv.CheckInLatitude = latitude;
            fv.CheckInLongitude = longitude;
            fv.Status = "CheckedIn";
            await Repository.UpdateAsync(fv);
            return await MapToGetOutputDtoAsync(fv);
        }

        public async Task<FieldVisitDto> CheckOutAsync(Guid id, double latitude, double longitude, string outcome, string notes)
        {
            var fv = await Repository.GetAsync(id);
            fv.CheckOutTime = DateTime.UtcNow;
            fv.CheckOutLatitude = latitude;
            fv.CheckOutLongitude = longitude;
            fv.Outcome = outcome;
            fv.Notes = notes;
            fv.Status = "Completed";
            await Repository.UpdateAsync(fv);
            return await MapToGetOutputDtoAsync(fv);
        }

        public async Task<ListResultDto<FieldVisitDto>> GetRoutePlanAsync(Guid employeeId, DateTime date)
        {
            var all = await Repository.GetListAsync();
            var filtered = all.Where(v => v.EmployeeId == employeeId && v.VisitDate.Date == date.Date)
                .OrderBy(v => v.VisitDate)
                .Select(v => new FieldVisitDto
                {
                    Id = v.Id,
                    EmployeeId = v.EmployeeId,
                    EmployeeName = v.EmployeeName,
                    CustomerId = v.CustomerId,
                    CustomerName = v.CustomerName,
                    VisitDate = v.VisitDate,
                    Purpose = v.Purpose,
                    CheckInTime = v.CheckInTime,
                    CheckInLatitude = v.CheckInLatitude,
                    CheckInLongitude = v.CheckInLongitude,
                    CheckOutTime = v.CheckOutTime,
                    CheckOutLatitude = v.CheckOutLatitude,
                    CheckOutLongitude = v.CheckOutLongitude,
                    Notes = v.Notes,
                    Outcome = v.Outcome,
                    Status = v.Status,
                    NextFollowUpDate = v.NextFollowUpDate
                }).ToList();
            return new ListResultDto<FieldVisitDto>(filtered);
        }

        protected override Task<FieldVisit> MapToEntityAsync(CreateFieldVisitDto createInput)
        {
            return Task.FromResult(new FieldVisit
            {
                EmployeeId = createInput.EmployeeId,
                EmployeeName = createInput.EmployeeName,
                CustomerId = createInput.CustomerId,
                CustomerName = createInput.CustomerName,
                VisitDate = createInput.VisitDate,
                Purpose = createInput.Purpose,
                Notes = createInput.Notes,
                Status = "Planned"
            });
        }

        protected override Task<FieldVisitDto> MapToGetOutputDtoAsync(FieldVisit entity)
        {
            return Task.FromResult(new FieldVisitDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = entity.EmployeeName,
                CustomerId = entity.CustomerId,
                CustomerName = entity.CustomerName,
                VisitDate = entity.VisitDate,
                Purpose = entity.Purpose,
                CheckInTime = entity.CheckInTime,
                CheckInLatitude = entity.CheckInLatitude,
                CheckInLongitude = entity.CheckInLongitude,
                CheckOutTime = entity.CheckOutTime,
                CheckOutLatitude = entity.CheckOutLatitude,
                CheckOutLongitude = entity.CheckOutLongitude,
                Notes = entity.Notes,
                Outcome = entity.Outcome,
                Status = entity.Status,
                NextFollowUpDate = entity.NextFollowUpDate
            });
        }
    }
}
