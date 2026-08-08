using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Inventory;

[DependsOn(
    typeof(InventoryDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class InventoryApplicationModule : AbpModule
{
}
