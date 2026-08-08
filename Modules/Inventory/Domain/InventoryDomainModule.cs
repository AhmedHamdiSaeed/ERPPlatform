using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Inventory;

[DependsOn(
    typeof(ERPPlatformDomainModule)
)]
public class InventoryDomainModule : AbpModule
{
}
