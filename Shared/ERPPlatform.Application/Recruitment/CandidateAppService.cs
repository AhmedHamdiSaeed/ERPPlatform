using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Recruitment
{
    public class CandidateDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied";
        public decimal Rating { get; set; }
        public List<string> Skills { get; set; } = new();
        public DateTime AppliedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateUpdateCandidateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied";
        public decimal Rating { get; set; }
        public List<string> Skills { get; set; } = new();
        public DateTime? AppliedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public interface ICandidateAppService : IApplicationService
    {
        Task<ListResultDto<CandidateDto>> GetListAsync(string? stage = null);
        Task<CandidateDto> GetAsync(Guid id);
        Task<CandidateDto> CreateAsync(CreateUpdateCandidateDto input);
        Task<CandidateDto> UpdateAsync(Guid id, CreateUpdateCandidateDto input);
        Task DeleteAsync(Guid id);
        Task<CandidateDto> UpdateStageAsync(Guid id, string newStage);
    }

    public class CandidateAppService : ApplicationService, ICandidateAppService
    {
        private static readonly string[] ValidStages =
            { "Applied", "Screening", "Interview", "Technical", "Offer", "Hired" };

        private readonly IRepository<Candidate, Guid> _candidateRepository;

        public CandidateAppService(IRepository<Candidate, Guid> candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public async Task<ListResultDto<CandidateDto>> GetListAsync(string? stage = null)
        {
            var query = await _candidateRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(stage))
            {
                query = query.Where(c => c.Stage == stage);
            }

            query = query.OrderByDescending(c => c.AppliedDate);

            var candidates = await AsyncExecuter.ToListAsync(query);
            return new ListResultDto<CandidateDto>(candidates.Select(MapToDto).ToList());
        }

        public async Task<CandidateDto> GetAsync(Guid id)
        {
            var candidate = await _candidateRepository.GetAsync(id);
            return MapToDto(candidate);
        }

        public async Task<CandidateDto> CreateAsync(CreateUpdateCandidateDto input)
        {
            var candidate = new Candidate
            {
                Name = input.Name,
                Email = input.Email,
                Phone = input.Phone,
                AppliedPosition = input.AppliedPosition,
                ExperienceYears = input.ExperienceYears,
                Stage = NormalizeStage(input.Stage),
                Rating = input.Rating,
                SkillsJson = SerializeSkills(input.Skills),
                AppliedDate = input.AppliedDate ?? DateTime.UtcNow,
                Notes = input.Notes
            };

            await _candidateRepository.InsertAsync(candidate);
            return MapToDto(candidate);
        }

        public async Task<CandidateDto> UpdateAsync(Guid id, CreateUpdateCandidateDto input)
        {
            var candidate = await _candidateRepository.GetAsync(id);

            candidate.Name = input.Name;
            candidate.Email = input.Email;
            candidate.Phone = input.Phone;
            candidate.AppliedPosition = input.AppliedPosition;
            candidate.ExperienceYears = input.ExperienceYears;
            candidate.Stage = NormalizeStage(input.Stage);
            candidate.Rating = input.Rating;
            candidate.SkillsJson = SerializeSkills(input.Skills);
            if (input.AppliedDate.HasValue) candidate.AppliedDate = input.AppliedDate.Value;
            candidate.Notes = input.Notes;

            await _candidateRepository.UpdateAsync(candidate);
            return MapToDto(candidate);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _candidateRepository.DeleteAsync(id);
        }

        public async Task<CandidateDto> UpdateStageAsync(Guid id, string newStage)
        {
            var candidate = await _candidateRepository.GetAsync(id);
            candidate.Stage = NormalizeStage(newStage);
            await _candidateRepository.UpdateAsync(candidate);
            return MapToDto(candidate);
        }

        private static string NormalizeStage(string? stage)
        {
            if (string.IsNullOrWhiteSpace(stage)) return "Applied";

            var match = ValidStages.FirstOrDefault(
                s => string.Equals(s, stage, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                throw new UserFriendlyException(
                    $"Unknown recruitment stage '{stage}'. Valid stages: {string.Join(", ", ValidStages)}.");
            }

            return match;
        }

        private static string SerializeSkills(List<string>? skills)
        {
            return JsonSerializer.Serialize(skills ?? new List<string>());
        }

        private static CandidateDto MapToDto(Candidate candidate)
        {
            List<string> skills;
            try
            {
                skills = string.IsNullOrWhiteSpace(candidate.SkillsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(candidate.SkillsJson) ?? new List<string>();
            }
            catch (JsonException)
            {
                skills = new List<string>();
            }

            return new CandidateDto
            {
                Id = candidate.Id,
                Name = candidate.Name,
                Email = candidate.Email,
                Phone = candidate.Phone,
                AppliedPosition = candidate.AppliedPosition,
                ExperienceYears = candidate.ExperienceYears,
                Stage = candidate.Stage,
                Rating = candidate.Rating,
                Skills = skills,
                AppliedDate = candidate.AppliedDate,
                Notes = candidate.Notes
            };
        }
    }
}
