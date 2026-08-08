using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.AI;

[DependsOn(
    typeof(ERPPlatformDomainModule)
)]
public class AIDomainModule : AbpModule
{
}
