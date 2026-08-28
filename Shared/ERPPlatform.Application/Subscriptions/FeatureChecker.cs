using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Subscriptions;

namespace ERPPlatform.Application.Subscriptions
{
    public interface IFeatureChecker
    {
        Task<bool> IsEnabledAsync(string featureCode, Guid? tenantId = null);
        Task<FeatureLimitResultDto> CheckLimitAsync(string featureCode, long requestedAmount = 1, Guid? tenantId = null);
    }

    public class FeatureChecker : ApplicationService, IFeatureChecker
    {
        private readonly IRepository<Subscription, Guid> _subscriptionRepository;
        private readonly IRepository<PlanFeature, Guid> _planFeatureRepository;
        private readonly IRepository<Feature, Guid> _featureRepository;
        private readonly IRepository<UsageRecord, Guid> _usageRepository;
        private readonly ICurrentTenant _currentTenant;

        public FeatureChecker(
            IRepository<Subscription, Guid> subscriptionRepository,
            IRepository<PlanFeature, Guid> planFeatureRepository,
            IRepository<Feature, Guid> featureRepository,
            IRepository<UsageRecord, Guid> usageRepository,
            ICurrentTenant currentTenant)
        {
            _subscriptionRepository = subscriptionRepository;
            _planFeatureRepository = planFeatureRepository;
            _featureRepository = featureRepository;
            _usageRepository = usageRepository;
            _currentTenant = currentTenant;
        }

        public async Task<bool> IsEnabledAsync(string featureCode, Guid? tenantId = null)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;

            // Get active subscription for tenant
            var subList = await _subscriptionRepository.GetListAsync(s => s.TenantId == targetTenantId && s.Status == "Active");
            var subscription = subList.FirstOrDefault();

            if (subscription == null)
            {
                // If no tenant subscription, fallback to default FREE plan check or false
                return false;
            }

            var feature = (await _featureRepository.GetListAsync(f => f.Code == featureCode)).FirstOrDefault();
            if (feature == null) return false;

            var planFeature = (await _planFeatureRepository.GetListAsync(pf => pf.PlanId == subscription.PlanId && pf.FeatureId == feature.Id)).FirstOrDefault();
            if (planFeature == null) return false;

            return planFeature.IsEnabled;
        }

        public async Task<FeatureLimitResultDto> CheckLimitAsync(string featureCode, long requestedAmount = 1, Guid? tenantId = null)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;

            var subList = await _subscriptionRepository.GetListAsync(s => s.TenantId == targetTenantId && s.Status == "Active");
            var subscription = subList.FirstOrDefault();

            var feature = (await _featureRepository.GetListAsync(f => f.Code == featureCode)).FirstOrDefault();
            if (feature == null)
            {
                return new FeatureLimitResultDto
                {
                    Allowed = false,
                    CurrentUsage = 0,
                    Limit = 0,
                    Remaining = 0,
                    IsUnlimited = false,
                    FeatureEnabled = false,
                    FeatureCode = featureCode
                };
            }

            if (subscription == null)
            {
                // Default fallback: allow if free trial or return standard limit
                return new FeatureLimitResultDto
                {
                    Allowed = true,
                    CurrentUsage = 0,
                    Limit = 100,
                    Remaining = 100,
                    IsUnlimited = false,
                    FeatureEnabled = true,
                    FeatureCode = featureCode
                };
            }

            var planFeature = (await _planFeatureRepository.GetListAsync(pf => pf.PlanId == subscription.PlanId && pf.FeatureId == feature.Id)).FirstOrDefault();

            if (planFeature == null || !planFeature.IsEnabled)
            {
                return new FeatureLimitResultDto
                {
                    Allowed = false,
                    CurrentUsage = 0,
                    Limit = 0,
                    Remaining = 0,
                    IsUnlimited = false,
                    FeatureEnabled = false,
                    FeatureCode = featureCode
                };
            }

            if (planFeature.LimitType == "Unlimited" || !planFeature.LimitValue.HasValue)
            {
                return new FeatureLimitResultDto
                {
                    Allowed = true,
                    CurrentUsage = 0,
                    Limit = null,
                    Remaining = null,
                    IsUnlimited = true,
                    FeatureEnabled = true,
                    FeatureCode = featureCode
                };
            }

            // Get current usage from UsageRecord repository (authoritative DB source)
            var start = subscription.CurrentPeriodStart;
            var end = subscription.CurrentPeriodEnd;

            var usageRecords = await _usageRepository.GetListAsync(u =>
                u.TenantId == targetTenantId &&
                u.FeatureCode == featureCode &&
                u.PeriodStart >= start.AddDays(-1) &&
                u.PeriodEnd <= end.AddDays(1));

            long currentUsage = usageRecords.FirstOrDefault()?.UsageValue ?? 0;
            long limit = planFeature.LimitValue.Value;
            long remaining = Math.Max(0, limit - currentUsage);
            bool allowed = (currentUsage + requestedAmount) <= limit;

            return new FeatureLimitResultDto
            {
                Allowed = allowed,
                CurrentUsage = currentUsage,
                Limit = limit,
                Remaining = remaining,
                IsUnlimited = false,
                FeatureEnabled = true,
                FeatureCode = featureCode
            };
        }
    }
}
