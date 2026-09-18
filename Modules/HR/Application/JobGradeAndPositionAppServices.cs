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
    public class JobGradeDto : EntityDto<Guid>
    {
        public string GradeCode { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Level { get; set; } = "Mid";
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateUpdateJobGradeDto
    {
        public string GradeCode { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Level { get; set; } = "Mid";
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class JobPositionDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateUpdateJobPositionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public interface IJobGradeAppService : ICrudAppService<JobGradeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateJobGradeDto>
    {
    }

    public class JobGradeAppService : CrudAppService<JobGrade, JobGradeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateJobGradeDto>, IJobGradeAppService
    {
        public JobGradeAppService(IRepository<JobGrade, Guid> repository) : base(repository)
        {
        }

        protected override Task<JobGrade> MapToEntityAsync(CreateUpdateJobGradeDto input)
        {
            return Task.FromResult(new JobGrade
            {
                GradeCode = input.GradeCode,
                GradeName = input.GradeName,
                Level = input.Level,
                MinSalary = input.MinSalary,
                MaxSalary = input.MaxSalary,
                Description = input.Description,
                IsActive = input.IsActive
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateJobGradeDto input, JobGrade entity)
        {
            entity.GradeCode = input.GradeCode;
            entity.GradeName = input.GradeName;
            entity.Level = input.Level;
            entity.MinSalary = input.MinSalary;
            entity.MaxSalary = input.MaxSalary;
            entity.Description = input.Description;
            entity.IsActive = input.IsActive;
            return Task.CompletedTask;
        }

        protected override Task<JobGradeDto> MapToGetOutputDtoAsync(JobGrade entity)
        {
            return Task.FromResult(new JobGradeDto
            {
                Id = entity.Id,
                GradeCode = entity.GradeCode,
                GradeName = entity.GradeName,
                Level = entity.Level,
                MinSalary = entity.MinSalary,
                MaxSalary = entity.MaxSalary,
                Description = entity.Description,
                IsActive = entity.IsActive
            });
        }
    }

    public interface IJobPositionAppService : ICrudAppService<JobPositionDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateJobPositionDto>
    {
    }

    public class JobPositionAppService : CrudAppService<JobPosition, JobPositionDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateJobPositionDto>, IJobPositionAppService
    {
        public JobPositionAppService(IRepository<JobPosition, Guid> repository) : base(repository)
        {
        }

        protected override Task<JobPosition> MapToEntityAsync(CreateUpdateJobPositionDto input)
        {
            return Task.FromResult(new JobPosition
            {
                Code = input.Code,
                Title = input.Title,
                DepartmentId = input.DepartmentId,
                DepartmentName = input.DepartmentName,
                JobGradeId = input.JobGradeId,
                JobGradeName = input.JobGradeName,
                Description = input.Description,
                Requirements = input.Requirements,
                MinSalary = input.MinSalary,
                MaxSalary = input.MaxSalary,
                IsActive = input.IsActive
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateJobPositionDto input, JobPosition entity)
        {
            entity.Code = input.Code;
            entity.Title = input.Title;
            entity.DepartmentId = input.DepartmentId;
            entity.DepartmentName = input.DepartmentName;
            entity.JobGradeId = input.JobGradeId;
            entity.JobGradeName = input.JobGradeName;
            entity.Description = input.Description;
            entity.Requirements = input.Requirements;
            entity.MinSalary = input.MinSalary;
            entity.MaxSalary = input.MaxSalary;
            entity.IsActive = input.IsActive;
            return Task.CompletedTask;
        }

        protected override Task<JobPositionDto> MapToGetOutputDtoAsync(JobPosition entity)
        {
            return Task.FromResult(new JobPositionDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Title = entity.Title,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.DepartmentName,
                JobGradeId = entity.JobGradeId,
                JobGradeName = entity.JobGradeName,
                Description = entity.Description,
                Requirements = entity.Requirements,
                MinSalary = entity.MinSalary,
                MaxSalary = entity.MaxSalary,
                IsActive = entity.IsActive
            });
        }
    }
}
