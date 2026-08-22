using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class DepartmentDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal Budget { get; set; }
    }

    public interface IDepartmentAppService : ICrudAppService<DepartmentDto, Guid, PagedAndSortedResultRequestDto, DepartmentDto>
    {
    }

    public class DepartmentAppService : CrudAppService<Department, DepartmentDto, Guid, PagedAndSortedResultRequestDto, DepartmentDto>, IDepartmentAppService
    {
        public DepartmentAppService(IRepository<Department, Guid> repository) : base(repository)
        {
        }

        protected override Task<Department> MapToEntityAsync(DepartmentDto createInput)
        {
            return Task.FromResult(new Department
            {
                Code = createInput.Code,
                Name = createInput.Name,
                Description = createInput.Description,
                ManagerName = createInput.ManagerName,
                EmployeeCount = createInput.EmployeeCount,
                Budget = createInput.Budget
            });
        }

        protected override Task MapToEntityAsync(DepartmentDto updateInput, Department entity)
        {
            entity.Code = updateInput.Code;
            entity.Name = updateInput.Name;
            entity.Description = updateInput.Description;
            entity.ManagerName = updateInput.ManagerName;
            entity.EmployeeCount = updateInput.EmployeeCount;
            entity.Budget = updateInput.Budget;
            return Task.CompletedTask;
        }

        protected override Task<DepartmentDto> MapToGetOutputDtoAsync(Department entity)
        {
            return Task.FromResult(new DepartmentDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                ManagerName = entity.ManagerName,
                EmployeeCount = entity.EmployeeCount,
                Budget = entity.Budget
            });
        }
    }
}
