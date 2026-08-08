using Volo.Abp.Modularity;
using ERPPlatform.EntityFrameworkCore;

namespace ERPPlatform.Modules.Inventory;

[DependsOn(
    typeof(InventoryDomainModule),
    typeof(ERPPlatformEntityFrameworkCoreModule)
)]
public class InventoryEntityFrameworkCoreModule : AbpModule
{
}
