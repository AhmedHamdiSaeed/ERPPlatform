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
    public class EmployeeContractDto : EntityDto<Guid>
    {
        public string ContractNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ContractType { get; set; } = "Permanent";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportationAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public int WorkingHoursPerWeek { get; set; }
        public int NoticePeriodDays { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? SignedAt { get; set; }
        public string SignedDocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int DaysUntilExpiration { get; set; }
    }

    public class CreateUpdateEmployeeContractDto
    {
        public string ContractNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ContractType { get; set; } = "Permanent";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportationAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public int WorkingHoursPerWeek { get; set; } = 40;
        public int NoticePeriodDays { get; set; } = 30;
        public string Status { get; set; } = "Active";
        public DateTime? SignedAt { get; set; }
        public string SignedDocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ContractRenewDto
    {
        public DateTime NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public decimal? NewBasicSalary { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public interface IContractAppService : ICrudAppService<EmployeeContractDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeContractDto>
    {
        Task<List<EmployeeContractDto>> GetContractsByEmployeeIdAsync(Guid employeeId);
        Task<List<EmployeeContractDto>> GetExpiringContractsAsync(int daysThreshold = 30);
        Task<EmployeeContractDto> RenewContractAsync(Guid id, ContractRenewDto input);
        Task<EmployeeContractDto> TerminateContractAsync(Guid id, string reason);
    }

    public class ContractAppService : CrudAppService<EmployeeContract, EmployeeContractDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeContractDto>, IContractAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public ContractAppService(
            IRepository<EmployeeContract, Guid> repository,
            IRepository<Employee, Guid> employeeRepository) : base(repository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<List<EmployeeContractDto>> GetContractsByEmployeeIdAsync(Guid employeeId)
        {
            var list = await Repository.GetListAsync(c => c.EmployeeId == employeeId);
            return list.OrderByDescending(c => c.StartDate).Select(MapEntityToDto).ToList();
        }

        public async Task<List<EmployeeContractDto>> GetExpiringContractsAsync(int daysThreshold = 30)
        {
            var now = DateTime.UtcNow;
            var target = now.AddDays(daysThreshold);
            var list = await Repository.GetListAsync(c => c.Status == "Active" && c.EndDate.HasValue && c.EndDate.Value >= now && c.EndDate.Value <= target);
            return list.OrderBy(c => c.EndDate).Select(MapEntityToDto).ToList();
        }

        public async Task<EmployeeContractDto> RenewContractAsync(Guid id, ContractRenewDto input)
        {
            var current = await Repository.GetAsync(id);
            current.Status = "Renewed";
            await Repository.UpdateAsync(current);

            var nextNum = $"CTR-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}";
            var renewed = new EmployeeContract
            {
                ContractNumber = nextNum,
                EmployeeId = current.EmployeeId,
                EmployeeName = current.EmployeeName,
                ContractType = current.ContractType,
                StartDate = input.NewStartDate,
                EndDate = input.NewEndDate,
                BasicSalary = input.NewBasicSalary ?? current.BasicSalary,
                HousingAllowance = current.HousingAllowance,
                TransportationAllowance = current.TransportationAllowance,
                OtherAllowances = current.OtherAllowances,
                WorkingHoursPerWeek = current.WorkingHoursPerWeek,
                NoticePeriodDays = current.NoticePeriodDays,
                Status = "Active",
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? $"Renewed from {current.ContractNumber}" : input.Notes
            };

            await Repository.InsertAsync(renewed);

            // Update Employee record if salary changed
            if (input.NewBasicSalary.HasValue)
            {
                var emp = await _employeeRepository.FindAsync(current.EmployeeId);
                if (emp != null)
                {
                    emp.Salary = renewed.TotalGrossSalary;
                    emp.ContractEndDate = input.NewEndDate;
                    await _employeeRepository.UpdateAsync(emp);
                }
            }

            return MapEntityToDto(renewed);
        }

        public async Task<EmployeeContractDto> TerminateContractAsync(Guid id, string reason)
        {
            var contract = await Repository.GetAsync(id);
            contract.Status = "Terminated";
            contract.Notes = (contract.Notes + $" | Terminated: {reason}").TrimStart('|', ' ');
            await Repository.UpdateAsync(contract);
            return MapEntityToDto(contract);
        }

        private static EmployeeContractDto MapEntityToDto(EmployeeContract c)
        {
            int days = 0;
            if (c.EndDate.HasValue && c.Status == "Active")
            {
                days = (int)(c.EndDate.Value.Date - DateTime.UtcNow.Date).TotalDays;
            }

            return new EmployeeContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.EmployeeName,
                ContractType = c.ContractType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ProbationEndDate = c.ProbationEndDate,
                BasicSalary = c.BasicSalary,
                HousingAllowance = c.HousingAllowance,
                TransportationAllowance = c.TransportationAllowance,
                OtherAllowances = c.OtherAllowances,
                TotalGrossSalary = c.BasicSalary + c.HousingAllowance + c.TransportationAllowance + c.OtherAllowances,
                WorkingHoursPerWeek = c.WorkingHoursPerWeek,
                NoticePeriodDays = c.NoticePeriodDays,
                Status = c.Status,
                SignedAt = c.SignedAt,
                SignedDocumentUrl = c.SignedDocumentUrl,
                Notes = c.Notes,
                DaysUntilExpiration = days
            };
        }

        protected override Task<EmployeeContract> MapToEntityAsync(CreateUpdateEmployeeContractDto input)
        {
            var code = string.IsNullOrWhiteSpace(input.ContractNumber) 
                ? $"CTR-{DateTime.UtcNow:yyyyMM}-{new Random().Next(1000, 9999)}"
                : input.ContractNumber;

            return Task.FromResult(new EmployeeContract
            {
                ContractNumber = code,
                EmployeeId = input.EmployeeId,
                EmployeeName = input.EmployeeName,
                ContractType = input.ContractType,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                ProbationEndDate = input.ProbationEndDate,
                BasicSalary = input.BasicSalary,
                HousingAllowance = input.HousingAllowance,
                TransportationAllowance = input.TransportationAllowance,
                OtherAllowances = input.OtherAllowances,
                WorkingHoursPerWeek = input.WorkingHoursPerWeek,
                NoticePeriodDays = input.NoticePeriodDays,
                Status = string.IsNullOrWhiteSpace(input.Status) ? "Active" : input.Status,
                SignedAt = input.SignedAt,
                SignedDocumentUrl = input.SignedDocumentUrl,
                Notes = input.Notes
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateEmployeeContractDto input, EmployeeContract entity)
        {
            entity.ContractNumber = input.ContractNumber;
            entity.EmployeeId = input.EmployeeId;
            entity.EmployeeName = input.EmployeeName;
            entity.ContractType = input.ContractType;
            entity.StartDate = input.StartDate;
            entity.EndDate = input.EndDate;
            entity.ProbationEndDate = input.ProbationEndDate;
            entity.BasicSalary = input.BasicSalary;
            entity.HousingAllowance = input.HousingAllowance;
            entity.TransportationAllowance = input.TransportationAllowance;
            entity.OtherAllowances = input.OtherAllowances;
            entity.WorkingHoursPerWeek = input.WorkingHoursPerWeek;
            entity.NoticePeriodDays = input.NoticePeriodDays;
            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                entity.Status = input.Status;
            }
            entity.SignedAt = input.SignedAt;
            entity.SignedDocumentUrl = input.SignedDocumentUrl;
            entity.Notes = input.Notes;
            return Task.CompletedTask;
        }

        protected override Task<EmployeeContractDto> MapToGetOutputDtoAsync(EmployeeContract entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
