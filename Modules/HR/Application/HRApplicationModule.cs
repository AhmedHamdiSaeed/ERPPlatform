using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.HR;

[DependsOn(
    typeof(HRDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class HRApplicationModule : AbpModule
{
}
