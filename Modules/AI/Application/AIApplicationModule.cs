using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.AI;

[DependsOn(
    typeof(AIDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class AIApplicationModule : AbpModule
{
}
