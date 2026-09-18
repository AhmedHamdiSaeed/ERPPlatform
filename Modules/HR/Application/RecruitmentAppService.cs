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
    public class JobRequisitionDto : EntityDto<Guid>
    {
        public string RequisitionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int VacanciesCount { get; set; }
        public string EmploymentType { get; set; } = "FullTime";
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string ExperienceLevel { get; set; } = "Mid";
        public string JobDescription { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string HiringManager { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime TargetStartDate { get; set; }
        public int CandidatesCount { get; set; }
    }

    public class CreateUpdateJobRequisitionDto
    {
        public string RequisitionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int VacanciesCount { get; set; } = 1;
        public string EmploymentType { get; set; } = "FullTime";
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string ExperienceLevel { get; set; } = "Mid";
        public string JobDescription { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string HiringManager { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime TargetStartDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    }

    public class CandidateDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public Guid? JobRequisitionId { get; set; }
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied";
        public decimal Rating { get; set; }
        public string SkillsJson { get; set; } = "[]";
        public DateTime AppliedDate { get; set; }
        public string CvUrl { get; set; } = string.Empty;
        public string ExpectedSalary { get; set; } = string.Empty;
        public string NoticePeriod { get; set; } = "1 Month";
        public string Notes { get; set; } = string.Empty;
        public Guid? ConvertedEmployeeId { get; set; }
    }

    public class CreateUpdateCandidateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public Guid? JobRequisitionId { get; set; }
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied";
        public decimal Rating { get; set; }
        public string SkillsJson { get; set; } = "[]";
        public string CvUrl { get; set; } = string.Empty;
        public string ExpectedSalary { get; set; } = string.Empty;
        public string NoticePeriod { get; set; } = "1 Month";
        public string Notes { get; set; } = string.Empty;
    }

    public class ConvertCandidateInputDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal AgreedBasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
        public string EmploymentType { get; set; } = "FullTime";
        public string JobGradeName { get; set; } = string.Empty;
    }

    public interface IRecruitmentAppService : ICrudAppService<CandidateDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCandidateDto>
    {
        Task<List<JobRequisitionDto>> GetRequisitionsAsync();
        Task<JobRequisitionDto> CreateRequisitionAsync(CreateUpdateJobRequisitionDto input);
        Task<CandidateDto> UpdateCandidateStageAsync(Guid id, string newStage);
        Task<EmployeeDto> ConvertCandidateToEmployeeAsync(Guid candidateId, ConvertCandidateInputDto input);
    }

    public class RecruitmentAppService : CrudAppService<Candidate, CandidateDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCandidateDto>, IRecruitmentAppService
    {
        private readonly IRepository<JobRequisition, Guid> _requisitionRepository;
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<EmployeeContract, Guid> _contractRepository;

        public RecruitmentAppService(
            IRepository<Candidate, Guid> repository,
            IRepository<JobRequisition, Guid> requisitionRepository,
            IRepository<Employee, Guid> employeeRepository,
            IRepository<EmployeeContract, Guid> contractRepository) : base(repository)
        {
            _requisitionRepository = requisitionRepository;
            _employeeRepository = employeeRepository;
            _contractRepository = contractRepository;
        }

        public async Task<List<JobRequisitionDto>> GetRequisitionsAsync()
        {
            var reqs = await _requisitionRepository.GetListAsync();
            var candidates = await Repository.GetListAsync();

            return reqs.OrderByDescending(r => r.CreationTime).Select(r => new JobRequisitionDto
            {
                Id = r.Id,
                RequisitionCode = r.RequisitionCode,
                Title = r.Title,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.DepartmentName,
                VacanciesCount = r.VacanciesCount,
                EmploymentType = r.EmploymentType,
                MinSalary = r.MinSalary,
                MaxSalary = r.MaxSalary,
                ExperienceLevel = r.ExperienceLevel,
                JobDescription = r.JobDescription,
                Requirements = r.Requirements,
                HiringManager = r.HiringManager,
                Status = r.Status,
                TargetStartDate = r.TargetStartDate,
                CandidatesCount = candidates.Count(c => c.JobRequisitionId == r.Id)
            }).ToList();
        }

        public async Task<JobRequisitionDto> CreateRequisitionAsync(CreateUpdateJobRequisitionDto input)
        {
            var code = string.IsNullOrWhiteSpace(input.RequisitionCode)
                ? $"REQ-{DateTime.UtcNow:yyyyMM}-{new Random().Next(100, 999)}"
                : input.RequisitionCode;

            var entity = new JobRequisition
            {
                RequisitionCode = code,
                Title = input.Title,
                DepartmentId = input.DepartmentId,
                DepartmentName = input.DepartmentName,
                VacanciesCount = input.VacanciesCount > 0 ? input.VacanciesCount : 1,
                EmploymentType = input.EmploymentType,
                MinSalary = input.MinSalary,
                MaxSalary = input.MaxSalary,
                ExperienceLevel = input.ExperienceLevel,
                JobDescription = input.JobDescription,
                Requirements = input.Requirements,
                HiringManager = input.HiringManager,
                Status = string.IsNullOrWhiteSpace(input.Status) ? "Open" : input.Status,
                TargetStartDate = input.TargetStartDate
            };

            await _requisitionRepository.InsertAsync(entity);

            return new JobRequisitionDto
            {
                Id = entity.Id,
                RequisitionCode = entity.RequisitionCode,
                Title = entity.Title,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.DepartmentName,
                VacanciesCount = entity.VacanciesCount,
                EmploymentType = entity.EmploymentType,
                MinSalary = entity.MinSalary,
                MaxSalary = entity.MaxSalary,
                ExperienceLevel = entity.ExperienceLevel,
                JobDescription = entity.JobDescription,
                Requirements = entity.Requirements,
                HiringManager = entity.HiringManager,
                Status = entity.Status,
                TargetStartDate = entity.TargetStartDate,
                CandidatesCount = 0
            };
        }

        public async Task<CandidateDto> UpdateCandidateStageAsync(Guid id, string newStage)
        {
            var cand = await Repository.GetAsync(id);
            cand.Stage = newStage;
            await Repository.UpdateAsync(cand);
            return MapEntityToDto(cand);
        }

        public async Task<EmployeeDto> ConvertCandidateToEmployeeAsync(Guid candidateId, ConvertCandidateInputDto input)
        {
            var cand = await Repository.GetAsync(candidateId);
            cand.Stage = "Hired";

            var empCode = string.IsNullOrWhiteSpace(input.EmployeeCode)
                ? $"EMP-{new Random().Next(1000, 9999)}"
                : input.EmployeeCode;

            var gross = input.AgreedBasicSalary + input.HousingAllowance + input.TransportAllowance;

            var employee = new Employee
            {
                EmployeeCode = empCode,
                Name = cand.Name,
                Email = cand.Email,
                Phone = cand.Phone,
                Position = cand.AppliedPosition,
                DepartmentId = input.DepartmentId,
                DepartmentName = input.DepartmentName,
                Salary = gross,
                JoiningDate = input.JoiningDate != default ? input.JoiningDate : DateTime.UtcNow,
                Status = "Active",
                EmploymentType = input.EmploymentType,
                JobGradeName = input.JobGradeName,
                Location = "Cairo HQ",
                LeaveBalance = 21.0m,
                ProbationEndDate = input.JoiningDate.AddMonths(3)
            };

            await _employeeRepository.InsertAsync(employee);

            // Generate first contract automatically
            var contract = new EmployeeContract
            {
                ContractNumber = $"CTR-{DateTime.UtcNow:yyyyMM}-{new Random().Next(1000, 9999)}",
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                ContractType = input.EmploymentType == "FullTime" ? "Permanent" : "FixedTerm",
                StartDate = input.JoiningDate,
                ProbationEndDate = input.JoiningDate.AddMonths(3),
                BasicSalary = input.AgreedBasicSalary,
                HousingAllowance = input.HousingAllowance,
                TransportationAllowance = input.TransportAllowance,
                WorkingHoursPerWeek = 40,
                NoticePeriodDays = 30,
                Status = "Active",
                Notes = $"Auto-generated from candidate conversion (Candidate: {cand.Name})"
            };

            await _contractRepository.InsertAsync(contract);

            cand.ConvertedEmployeeId = employee.Id;
            await Repository.UpdateAsync(cand);

            return new EmployeeDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                Name = employee.Name,
                Email = employee.Email,
                Phone = employee.Phone,
                Position = employee.Position,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.DepartmentName,
                Salary = employee.Salary,
                JoiningDate = employee.JoiningDate,
                Status = employee.Status,
                EmploymentType = employee.EmploymentType,
                JobGradeName = employee.JobGradeName,
                LeaveBalance = employee.LeaveBalance
            };
        }

        private static CandidateDto MapEntityToDto(Candidate c)
        {
            return new CandidateDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                AppliedPosition = c.AppliedPosition,
                JobRequisitionId = c.JobRequisitionId,
                ExperienceYears = c.ExperienceYears,
                Stage = c.Stage,
                Rating = c.Rating,
                SkillsJson = c.SkillsJson,
                AppliedDate = c.AppliedDate,
                CvUrl = c.CvUrl,
                ExpectedSalary = c.ExpectedSalary,
                NoticePeriod = c.NoticePeriod,
                Notes = c.Notes,
                ConvertedEmployeeId = c.ConvertedEmployeeId
            };
        }

        protected override Task<Candidate> MapToEntityAsync(CreateUpdateCandidateDto input)
        {
            return Task.FromResult(new Candidate
            {
                Name = input.Name,
                Email = input.Email,
                Phone = input.Phone,
                AppliedPosition = input.AppliedPosition,
                JobRequisitionId = input.JobRequisitionId,
                ExperienceYears = input.ExperienceYears,
                Stage = string.IsNullOrWhiteSpace(input.Stage) ? "Applied" : input.Stage,
                Rating = input.Rating,
                SkillsJson = input.SkillsJson,
                CvUrl = input.CvUrl,
                ExpectedSalary = input.ExpectedSalary,
                NoticePeriod = input.NoticePeriod,
                Notes = input.Notes,
                AppliedDate = DateTime.UtcNow
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateCandidateDto input, Candidate entity)
        {
            entity.Name = input.Name;
            entity.Email = input.Email;
            entity.Phone = input.Phone;
            entity.AppliedPosition = input.AppliedPosition;
            entity.JobRequisitionId = input.JobRequisitionId;
            entity.ExperienceYears = input.ExperienceYears;
            if (!string.IsNullOrWhiteSpace(input.Stage)) entity.Stage = input.Stage;
            entity.Rating = input.Rating;
            entity.SkillsJson = input.SkillsJson;
            entity.CvUrl = input.CvUrl;
            entity.ExpectedSalary = input.ExpectedSalary;
            entity.NoticePeriod = input.NoticePeriod;
            entity.Notes = input.Notes;
            return Task.CompletedTask;
        }

        protected override Task<CandidateDto> MapToGetOutputDtoAsync(Candidate entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
