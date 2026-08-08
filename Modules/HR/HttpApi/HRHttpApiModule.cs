using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.HR;

[DependsOn(
    typeof(HRApplicationModule),
    typeof(ERPPlatformHttpApiModule)
)]
public class HRHttpApiModule : AbpModule
{
}
