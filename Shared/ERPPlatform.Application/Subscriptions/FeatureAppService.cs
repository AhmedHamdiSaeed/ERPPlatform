using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Application.Subscriptions
{
    public interface IFeatureAppService : ICrudAppService<FeatureDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateFeatureDto>
    {
    }

    public class FeatureAppService : CrudAppService<Feature, FeatureDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateFeatureDto>, IFeatureAppService
    {
        public FeatureAppService(IRepository<Feature, Guid> repository) : base(repository)
        {
        }
    }
}
