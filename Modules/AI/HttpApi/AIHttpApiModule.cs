using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.AI;

[DependsOn(
    typeof(AIApplicationModule),
    typeof(ERPPlatformHttpApiModule)
)]
public class AIHttpApiModule : AbpModule
{
}
