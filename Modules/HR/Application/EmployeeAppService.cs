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
    public class EmployeeDto : EntityDto<Guid>
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
    }

    public class CreateUpdateEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Status { get; set; } = "Active";
    }

    public interface IEmployeeAppService : ICrudAppService<EmployeeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDto>
    {
    }

    public class EmployeeAppService : CrudAppService<Employee, EmployeeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDto>, IEmployeeAppService
    {
        public EmployeeAppService(IRepository<Employee, Guid> repository) : base(repository)
        {
        }

        protected override Task<Employee> MapToEntityAsync(CreateUpdateEmployeeDto createInput)
        {
            return Task.FromResult(new Employee
            {
                EmployeeCode = createInput.EmployeeCode,
                Name = createInput.Name,
                Email = createInput.Email,
                Phone = createInput.Phone,
                Position = createInput.Position,
                DepartmentName = createInput.DepartmentName,
                Salary = createInput.Salary,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Active" : createInput.Status,
                Location = "Cairo HQ",
                JoiningDate = DateTime.UtcNow
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateEmployeeDto updateInput, Employee entity)
        {
            entity.EmployeeCode = updateInput.EmployeeCode;
            entity.Name = updateInput.Name;
            entity.Email = updateInput.Email;
            entity.Phone = updateInput.Phone;
            entity.Position = updateInput.Position;
            entity.DepartmentName = updateInput.DepartmentName;
            entity.Salary = updateInput.Salary;
            if (!string.IsNullOrWhiteSpace(updateInput.Status))
            {
                entity.Status = updateInput.Status;
            }
            return Task.CompletedTask;
        }

        protected override Task<EmployeeDto> MapToGetOutputDtoAsync(Employee entity)
        {
            return Task.FromResult(new EmployeeDto
            {
                Id = entity.Id,
                EmployeeCode = entity.EmployeeCode,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Position = entity.Position,
                DepartmentName = entity.DepartmentName,
                Salary = entity.Salary,
                JoiningDate = entity.JoiningDate,
                Status = entity.Status,
                Avatar = entity.Avatar,
                ManagerName = entity.ManagerName,
                Location = entity.Location
            });
        }
    }
}
