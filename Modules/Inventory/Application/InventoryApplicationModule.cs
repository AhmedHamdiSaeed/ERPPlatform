using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ERPPlatform.Modules.Inventory;

[DependsOn(
    typeof(InventoryDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class InventoryApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<InventoryApplicationModule>();
        });
    }
}
