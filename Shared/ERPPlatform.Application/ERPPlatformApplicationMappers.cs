using AutoMapper;
using ERPPlatform.Application.Subscriptions;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Integrations;

namespace ERPPlatform;

public class ERPPlatformApplicationAutoMapperProfile : Profile
{
    public ERPPlatformApplicationAutoMapperProfile()
    {
        CreateMap<Plan, PlanDto>().ReverseMap();
        CreateMap<CreateUpdatePlanDto, Plan>().ReverseMap();
        CreateMap<Feature, FeatureDto>().ReverseMap();
        CreateMap<CreateUpdateFeatureDto, Feature>().ReverseMap();
        CreateMap<IntegrationConfig, IntegrationConfigDto>().ReverseMap();
        CreateMap<CreateUpdateIntegrationConfigDto, IntegrationConfig>().ReverseMap();
    }
}
