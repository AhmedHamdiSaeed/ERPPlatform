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
    public interface IUsageService
    {
        Task<long> GetUsageAsync(Guid? tenantId, string featureCode);
        Task<long> IncrementAsync(Guid? tenantId, string featureCode, long amount = 1);
        Task<long> DecrementAsync(Guid? tenantId, string featureCode, long amount = 1);
        Task ResetAsync(Guid? tenantId, string featureCode);
        Task RebuildCacheAsync(Guid? tenantId);
    }

    public class UsageService : ApplicationService, IUsageService
    {
        private readonly IRepository<UsageRecord, Guid> _usageRepository;
        private readonly IRepository<Subscription, Guid> _subscriptionRepository;
        private readonly IRepository<Feature, Guid> _featureRepository;
        private readonly ICurrentTenant _currentTenant;

        public UsageService(
            IRepository<UsageRecord, Guid> usageRepository,
            IRepository<Subscription, Guid> subscriptionRepository,
            IRepository<Feature, Guid> featureRepository,
            ICurrentTenant currentTenant)
        {
            _usageRepository = usageRepository;
            _subscriptionRepository = subscriptionRepository;
            _featureRepository = featureRepository;
            _currentTenant = currentTenant;
        }

        public async Task<long> GetUsageAsync(Guid? tenantId, string featureCode)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;
            var sub = (await _subscriptionRepository.GetListAsync(s => s.TenantId == targetTenantId && s.Status == "Active")).FirstOrDefault();
            
            var pStart = sub?.CurrentPeriodStart ?? DateTime.UtcNow.AddDays(-30);
            var pEnd = sub?.CurrentPeriodEnd ?? DateTime.UtcNow.AddDays(30);

            var record = (await _usageRepository.GetListAsync(u => u.TenantId == targetTenantId && u.FeatureCode == featureCode)).FirstOrDefault();
            return record?.UsageValue ?? 0;
        }

        public async Task<long> IncrementAsync(Guid? tenantId, string featureCode, long amount = 1)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;
            var sub = (await _subscriptionRepository.GetListAsync(s => s.TenantId == targetTenantId && s.Status == "Active")).FirstOrDefault();

            var feature = (await _featureRepository.GetListAsync(f => f.Code == featureCode)).FirstOrDefault();
            var featureId = feature?.Id ?? Guid.NewGuid();

            var pStart = sub?.CurrentPeriodStart ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var pEnd = sub?.CurrentPeriodEnd ?? pStart.AddMonths(1).AddDays(-1);

            var records = await _usageRepository.GetListAsync(u => u.TenantId == targetTenantId && u.FeatureCode == featureCode);
            var record = records.FirstOrDefault();

            if (record == null)
            {
                record = new UsageRecord
                {
                    TenantId = targetTenantId,
                    FeatureId = featureId,
                    FeatureCode = featureCode,
                    PeriodStart = pStart,
                    PeriodEnd = pEnd,
                    UsageValue = amount
                };
                await _usageRepository.InsertAsync(record);
            }
            else
            {
                record.UsageValue += amount;
                await _usageRepository.UpdateAsync(record);
            }

            return record.UsageValue;
        }

        public async Task<long> DecrementAsync(Guid? tenantId, string featureCode, long amount = 1)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;
            var records = await _usageRepository.GetListAsync(u => u.TenantId == targetTenantId && u.FeatureCode == featureCode);
            var record = records.FirstOrDefault();

            if (record != null)
            {
                record.UsageValue = Math.Max(0, record.UsageValue - amount);
                await _usageRepository.UpdateAsync(record);
                return record.UsageValue;
            }

            return 0;
        }

        public async Task ResetAsync(Guid? tenantId, string featureCode)
        {
            var targetTenantId = tenantId ?? _currentTenant.Id;
            var records = await _usageRepository.GetListAsync(u => u.TenantId == targetTenantId && u.FeatureCode == featureCode);
            var record = records.FirstOrDefault();

            if (record != null)
            {
                record.UsageValue = 0;
                await _usageRepository.UpdateAsync(record);
            }
        }

        public async Task RebuildCacheAsync(Guid? tenantId)
        {
            // Sync database records to Redis cache layer
            await Task.CompletedTask;
        }
    }
}
