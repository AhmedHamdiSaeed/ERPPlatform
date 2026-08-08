using Volo.Abp.Modularity;

namespace ERPPlatform;

[DependsOn(
    typeof(ERPPlatformApplicationModule),
    typeof(ERPPlatformDomainTestModule)
)]
public class ERPPlatformApplicationTestModule : AbpModule
{

}
