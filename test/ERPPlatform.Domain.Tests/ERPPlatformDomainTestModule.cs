using Volo.Abp.Modularity;

namespace ERPPlatform;

[DependsOn(
    typeof(ERPPlatformDomainModule),
    typeof(ERPPlatformTestBaseModule)
)]
public class ERPPlatformDomainTestModule : AbpModule
{

}
