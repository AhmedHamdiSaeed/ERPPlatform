using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Inventory;

[DependsOn(
    typeof(InventoryApplicationModule),
    typeof(ERPPlatformHttpApiModule)
)]
public class InventoryHttpApiModule : AbpModule
{
}
