using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ERPPlatform.Modules.HR;

[DependsOn(
    typeof(HRDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class HRApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<HRApplicationModule>();
        });
    }
}
