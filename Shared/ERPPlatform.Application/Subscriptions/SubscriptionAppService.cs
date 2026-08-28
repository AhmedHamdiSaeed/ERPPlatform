using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Subscriptions;

namespace ERPPlatform.Application.Subscriptions
{
    public interface ISubscriptionAppService
    {
        Task<SubscriptionDto> GetCurrentAsync();
        Task<List<PlanDto>> GetPlansAsync();
        Task<SubscriptionDto> ChangePlanAsync(Guid newPlanId);
        Task CancelAsync();
        Task ResumeAsync();
        Task<List<FeatureAccessDto>> GetFeaturesAsync();
    }

    public class SubscriptionAppService : ApplicationService, ISubscriptionAppService
    {
        private readonly IRepository<Subscription, Guid> _subscriptionRepository;
        private readonly IRepository<SubscriptionHistory, Guid> _historyRepository;
        private readonly IRepository<Plan, Guid> _planRepository;
        private readonly IRepository<Feature, Guid> _featureRepository;
        private readonly IRepository<PlanFeature, Guid> _planFeatureRepository;
        private readonly IRepository<UsageRecord, Guid> _usageRepository;
        private readonly ICurrentTenant _currentTenant;

        public SubscriptionAppService(
            IRepository<Subscription, Guid> subscriptionRepository,
            IRepository<SubscriptionHistory, Guid> historyRepository,
            IRepository<Plan, Guid> planRepository,
            IRepository<Feature, Guid> featureRepository,
            IRepository<PlanFeature, Guid> planFeatureRepository,
            IRepository<UsageRecord, Guid> usageRepository,
            ICurrentTenant currentTenant)
        {
            _subscriptionRepository = subscriptionRepository;
            _historyRepository = historyRepository;
            _planRepository = planRepository;
            _featureRepository = featureRepository;
            _planFeatureRepository = planFeatureRepository;
            _usageRepository = usageRepository;
            _currentTenant = currentTenant;
        }

        public async Task<SubscriptionDto> GetCurrentAsync()
        {
            var tenantId = _currentTenant.Id;
            var subs = await _subscriptionRepository.GetListAsync(s => s.TenantId == tenantId && s.Status == "Active");
            var sub = subs.FirstOrDefault();

            if (sub == null)
            {
                // Fallback / Auto-assign Default FREE plan
                var freePlan = (await _planRepository.GetListAsync(p => p.Code == "FREE")).FirstOrDefault();
                var planId = freePlan?.Id ?? Guid.NewGuid();

                sub = new Subscription
                {
                    TenantId = tenantId,
                    PlanId = planId,
                    PlanCode = freePlan?.Code ?? "FREE",
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                    AutoRenew = true
                };
                await _subscriptionRepository.InsertAsync(sub);
            }

            var plan = await _planRepository.FindAsync(sub.PlanId);

            return new SubscriptionDto
            {
                Id = sub.Id,
                TenantId = sub.TenantId,
                PlanId = sub.PlanId,
                PlanName = plan?.Name ?? "Free Plan",
                PlanCode = plan?.Code ?? "FREE",
                PlanPrice = plan?.Price ?? 0,
                Status = sub.Status,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                CurrentPeriodStart = sub.CurrentPeriodStart,
                CurrentPeriodEnd = sub.CurrentPeriodEnd,
                AutoRenew = sub.AutoRenew
            };
        }

        public async Task<List<PlanDto>> GetPlansAsync()
        {
            var plans = await _planRepository.GetListAsync(p => p.IsActive && p.IsPublic);
            return plans.OrderBy(p => p.DisplayOrder).Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Price = p.Price,
                Currency = p.Currency,
                BillingPeriod = p.BillingPeriod,
                IsActive = p.IsActive,
                IsPublic = p.IsPublic,
                DisplayOrder = p.DisplayOrder
            }).ToList();
        }

        public async Task<SubscriptionDto> ChangePlanAsync(Guid newPlanId)
        {
            var tenantId = _currentTenant.Id;
            var sub = (await _subscriptionRepository.GetListAsync(s => s.TenantId == tenantId)).FirstOrDefault();
            var newPlan = await _planRepository.GetAsync(newPlanId);

            Guid? oldPlanId = sub?.PlanId;

            if (sub == null)
            {
                sub = new Subscription
                {
                    TenantId = tenantId,
                    PlanId = newPlanId,
                    PlanCode = newPlan.Code,
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
                };
                await _subscriptionRepository.InsertAsync(sub);
            }
            else
            {
                sub.PlanId = newPlanId;
                sub.PlanCode = newPlan.Code;
                sub.Status = "Active";
                await _subscriptionRepository.UpdateAsync(sub);
            }

            await _historyRepository.InsertAsync(new SubscriptionHistory
            {
                TenantId = tenantId,
                SubscriptionId = sub.Id,
                OldPlanId = oldPlanId,
                NewPlanId = newPlanId,
                Action = oldPlanId == null ? "Created" : "Upgraded",
                ChangedAt = DateTime.UtcNow,
                Reason = "Tenant self-service plan change"
            });

            return await GetCurrentAsync();
        }

        public async Task CancelAsync()
        {
            var tenantId = _currentTenant.Id;
            var sub = (await _subscriptionRepository.GetListAsync(s => s.TenantId == tenantId)).FirstOrDefault();
            if (sub != null)
            {
                sub.Status = "Cancelled";
                sub.CancelledAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
            }
        }

        public async Task ResumeAsync()
        {
            var tenantId = _currentTenant.Id;
            var sub = (await _subscriptionRepository.GetListAsync(s => s.TenantId == tenantId)).FirstOrDefault();
            if (sub != null)
            {
                sub.Status = "Active";
                sub.CancelledAt = null;
                await _subscriptionRepository.UpdateAsync(sub);
            }
        }

        public async Task<List<FeatureAccessDto>> GetFeaturesAsync()
        {
            var currentSub = await GetCurrentAsync();
            var planFeatures = await _planFeatureRepository.GetListAsync(pf => pf.PlanId == currentSub.PlanId);
            var features = await _featureRepository.GetListAsync();
            var usages = await _usageRepository.GetListAsync(u => u.TenantId == currentSub.TenantId);

            var list = new List<FeatureAccessDto>();
            foreach (var f in features)
            {
                var pf = planFeatures.FirstOrDefault(x => x.FeatureId == f.Id);
                var u = usages.FirstOrDefault(x => x.FeatureCode == f.Code);

                long usageVal = u?.UsageValue ?? 0;
                long? limitVal = pf?.LimitValue;
                long? remaining = limitVal.HasValue ? Math.Max(0, limitVal.Value - usageVal) : null;

                list.Add(new FeatureAccessDto
                {
                    Code = f.Code,
                    Name = f.Name,
                    Enabled = pf?.IsEnabled ?? false,
                    Limit = limitVal,
                    Usage = usageVal,
                    Remaining = remaining,
                    Unit = f.Unit,
                    Category = f.Category
                });
            }

            return list;
        }
    }
}
