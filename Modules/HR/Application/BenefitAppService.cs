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
    public class BenefitPlanDto : EntityDto<Guid>
    {
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Category { get; set; } = "MedicalInsurance";
        public string ProviderName { get; set; } = string.Empty;
        public string CoverageDetails { get; set; } = string.Empty;
        public decimal EmployerContributionMonthly { get; set; }
        public decimal EmployeeContributionMonthly { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateBenefitPlanDto
    {
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Category { get; set; } = "MedicalInsurance";
        public string ProviderName { get; set; } = string.Empty;
        public string CoverageDetails { get; set; } = string.Empty;
        public decimal EmployerContributionMonthly { get; set; }
        public decimal EmployeeContributionMonthly { get; set; }
    }

    public class EmployeeBenefitDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid BenefitPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string Category { get; set; } = "MedicalInsurance";
        public DateTime EnrollmentDate { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeDeduction { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class EnrollEmployeeBenefitDto
    {
        public Guid EmployeeId { get; set; }
        public Guid BenefitPlanId { get; set; }
        public decimal CoverageAmount { get; set; } = 50000;
    }

    public interface IBenefitAppService : IApplicationService
    {
        Task<List<BenefitPlanDto>> GetPlansAsync();
        Task<BenefitPlanDto> CreatePlanAsync(CreateBenefitPlanDto input);
        Task<List<EmployeeBenefitDto>> GetEmployeeBenefitsAsync(Guid? employeeId = null);
        Task<EmployeeBenefitDto> EnrollEmployeeAsync(EnrollEmployeeBenefitDto input);
        Task<EmployeeBenefitDto> TerminateBenefitAsync(Guid benefitId);
    }

    public class BenefitAppService : ApplicationService, IBenefitAppService
    {
        private readonly IRepository<BenefitPlan, Guid> _planRepo;
        private readonly IRepository<EmployeeBenefit, Guid> _benefitRepo;
        private readonly IRepository<Employee, Guid> _employeeRepo;

        public BenefitAppService(
            IRepository<BenefitPlan, Guid> planRepo,
            IRepository<EmployeeBenefit, Guid> benefitRepo,
            IRepository<Employee, Guid> employeeRepo)
        {
            _planRepo = planRepo;
            _benefitRepo = benefitRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<BenefitPlanDto>> GetPlansAsync()
        {
            var plans = await _planRepo.GetListAsync();
            return plans.Select(p => new BenefitPlanDto
            {
                Id = p.Id,
                PlanCode = p.PlanCode,
                PlanName = p.PlanName,
                Category = p.Category,
                ProviderName = p.ProviderName,
                CoverageDetails = p.CoverageDetails,
                EmployerContributionMonthly = p.EmployerContributionMonthly,
                EmployeeContributionMonthly = p.EmployeeContributionMonthly,
                IsActive = p.IsActive
            }).ToList();
        }

        public async Task<BenefitPlanDto> CreatePlanAsync(CreateBenefitPlanDto input)
        {
            var plan = new BenefitPlan
            {
                PlanCode = !string.IsNullOrWhiteSpace(input.PlanCode) ? input.PlanCode : $"BEN-{DateTime.UtcNow.Ticks % 10000:D4}",
                PlanName = input.PlanName,
                Category = input.Category,
                ProviderName = input.ProviderName,
                CoverageDetails = input.CoverageDetails,
                EmployerContributionMonthly = input.EmployerContributionMonthly,
                EmployeeContributionMonthly = input.EmployeeContributionMonthly,
                IsActive = true
            };

            await _planRepo.InsertAsync(plan);
            return new BenefitPlanDto
            {
                Id = plan.Id,
                PlanCode = plan.PlanCode,
                PlanName = plan.PlanName,
                Category = plan.Category,
                ProviderName = plan.ProviderName,
                CoverageDetails = plan.CoverageDetails,
                EmployerContributionMonthly = plan.EmployerContributionMonthly,
                EmployeeContributionMonthly = plan.EmployeeContributionMonthly,
                IsActive = plan.IsActive
            };
        }

        public async Task<List<EmployeeBenefitDto>> GetEmployeeBenefitsAsync(Guid? employeeId = null)
        {
            var query = await _benefitRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
                query = query.Where(b => b.EmployeeId == employeeId.Value);

            var list = query.OrderByDescending(b => b.EnrollmentDate).ToList();
            return list.Select(b => new EmployeeBenefitDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                EmployeeName = b.EmployeeName,
                BenefitPlanId = b.BenefitPlanId,
                PlanName = b.PlanName,
                Category = b.Category,
                EnrollmentDate = b.EnrollmentDate,
                CoverageAmount = b.CoverageAmount,
                EmployerContribution = b.EmployerContribution,
                EmployeeDeduction = b.EmployeeDeduction,
                Status = b.Status
            }).ToList();
        }

        public async Task<EmployeeBenefitDto> EnrollEmployeeAsync(EnrollEmployeeBenefitDto input)
        {
            var emp = await _employeeRepo.GetAsync(input.EmployeeId);
            var plan = await _planRepo.GetAsync(input.BenefitPlanId);

            var benefit = new EmployeeBenefit
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                BenefitPlanId = plan.Id,
                PlanName = plan.PlanName,
                Category = plan.Category,
                EnrollmentDate = DateTime.UtcNow,
                CoverageAmount = input.CoverageAmount,
                EmployerContribution = plan.EmployerContributionMonthly,
                EmployeeDeduction = plan.EmployeeContributionMonthly,
                Status = "Active"
            };

            await _benefitRepo.InsertAsync(benefit);
            return new EmployeeBenefitDto
            {
                Id = benefit.Id,
                EmployeeId = benefit.EmployeeId,
                EmployeeName = benefit.EmployeeName,
                BenefitPlanId = benefit.BenefitPlanId,
                PlanName = benefit.PlanName,
                Category = benefit.Category,
                EnrollmentDate = benefit.EnrollmentDate,
                CoverageAmount = benefit.CoverageAmount,
                EmployerContribution = benefit.EmployerContribution,
                EmployeeDeduction = benefit.EmployeeDeduction,
                Status = benefit.Status
            };
        }

        public async Task<EmployeeBenefitDto> TerminateBenefitAsync(Guid benefitId)
        {
            var benefit = await _benefitRepo.GetAsync(benefitId);
            benefit.Status = "Terminated";
            await _benefitRepo.UpdateAsync(benefit);
            return new EmployeeBenefitDto
            {
                Id = benefit.Id,
                EmployeeId = benefit.EmployeeId,
                EmployeeName = benefit.EmployeeName,
                BenefitPlanId = benefit.BenefitPlanId,
                PlanName = benefit.PlanName,
                Category = benefit.Category,
                EnrollmentDate = benefit.EnrollmentDate,
                CoverageAmount = benefit.CoverageAmount,
                EmployerContribution = benefit.EmployerContribution,
                EmployeeDeduction = benefit.EmployeeDeduction,
                Status = benefit.Status
            };
        }
    }
}
