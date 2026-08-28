using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Application.Subscriptions
{
    public interface IPlanAppService : ICrudAppService<PlanDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePlanDto>
    {
        Task SetActiveAsync(Guid id, bool isActive);
        Task<List<PlanFeatureDto>> GetFeaturesAsync(Guid planId);
        Task UpdateFeaturesAsync(Guid planId, List<SetPlanFeatureDto> features);
    }

    public class PlanAppService : CrudAppService<Plan, PlanDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePlanDto>, IPlanAppService
    {
        private readonly IRepository<PlanFeature, Guid> _planFeatureRepository;
        private readonly IRepository<Feature, Guid> _featureRepository;

        public PlanAppService(
            IRepository<Plan, Guid> repository,
            IRepository<PlanFeature, Guid> planFeatureRepository,
            IRepository<Feature, Guid> featureRepository) : base(repository)
        {
            _planFeatureRepository = planFeatureRepository;
            _featureRepository = featureRepository;
        }

        public async Task SetActiveAsync(Guid id, bool isActive)
        {
            var plan = await Repository.GetAsync(id);
            plan.IsActive = isActive;
            await Repository.UpdateAsync(plan);
        }

        public async Task<List<PlanFeatureDto>> GetFeaturesAsync(Guid planId)
        {
            var allFeatures = await _featureRepository.GetListAsync();
            var planFeatures = await _planFeatureRepository.GetListAsync(pf => pf.PlanId == planId);

            var result = new List<PlanFeatureDto>();
            foreach (var f in allFeatures)
            {
                var pf = planFeatures.FirstOrDefault(x => x.FeatureId == f.Id);
                result.Add(new PlanFeatureDto
                {
                    Id = pf?.Id ?? Guid.Empty,
                    PlanId = planId,
                    FeatureId = f.Id,
                    FeatureCode = f.Code,
                    FeatureName = f.Name,
                    IsEnabled = pf?.IsEnabled ?? false,
                    LimitValue = pf?.LimitValue,
                    LimitType = pf?.LimitType ?? "Monthly",
                    Unit = f.Unit
                });
            }

            return result;
        }

        public async Task UpdateFeaturesAsync(Guid planId, List<SetPlanFeatureDto> features)
        {
            var existing = await _planFeatureRepository.GetListAsync(pf => pf.PlanId == planId);
            foreach (var item in existing)
            {
                await _planFeatureRepository.DeleteAsync(item);
            }

            foreach (var f in features)
            {
                var featureEntity = await _featureRepository.GetAsync(f.FeatureId);
                await _planFeatureRepository.InsertAsync(new PlanFeature
                {
                    PlanId = planId,
                    FeatureId = f.FeatureId,
                    FeatureCode = featureEntity.Code,
                    IsEnabled = f.IsEnabled,
                    LimitValue = f.LimitValue,
                    LimitType = f.LimitType
                });
            }
        }
    }
}
