using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Integrations
{
    public class IntegrationConfigDto : EntityDto<Guid>
    {
        public string ProviderType { get; set; } = "PaymentGateway";
        public string ProviderName { get; set; } = "Stripe";
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ConfigJson { get; set; } = "{}";
    }

    public class CreateUpdateIntegrationConfigDto
    {
        public string ProviderType { get; set; } = "PaymentGateway";
        public string ProviderName { get; set; } = "Stripe";
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ConfigJson { get; set; } = "{}";
    }

    public interface IIntegrationConfigAppService : ICrudAppService<IntegrationConfigDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateIntegrationConfigDto>
    {
        Task<bool> TestConnectionAsync(Guid id);
        Task<List<IntegrationConfigDto>> GetActiveProvidersAsync();
    }

    public class IntegrationConfigAppService : CrudAppService<IntegrationConfig, IntegrationConfigDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateIntegrationConfigDto>, IIntegrationConfigAppService
    {
        public IntegrationConfigAppService(IRepository<IntegrationConfig, Guid> repository) : base(repository)
        {
        }

        public async Task<bool> TestConnectionAsync(Guid id)
        {
            var config = await Repository.GetAsync(id);
            if (config == null || !config.IsActive)
            {
                return false;
            }

            // Simulate provider connection check
            return !string.IsNullOrWhiteSpace(config.ApiKey) || !string.IsNullOrWhiteSpace(config.SecretKey);
        }

        public async Task<List<IntegrationConfigDto>> GetActiveProvidersAsync()
        {
            var list = await Repository.GetListAsync(x => x.IsActive);
            return list.Select(MapToGetOutputDto).ToList();
        }
    }
}
