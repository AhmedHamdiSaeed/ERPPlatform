using Volo.Abp.Modularity;
using ERPPlatform.EntityFrameworkCore;

namespace ERPPlatform.Modules.AI;

[DependsOn(
    typeof(AIDomainModule),
    typeof(ERPPlatformEntityFrameworkCoreModule)
)]
public class AIEntityFrameworkCoreModule : AbpModule
{
}
