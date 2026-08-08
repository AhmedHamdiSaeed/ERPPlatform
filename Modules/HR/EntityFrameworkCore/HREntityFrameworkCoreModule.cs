using Volo.Abp.Modularity;
using ERPPlatform.EntityFrameworkCore;

namespace ERPPlatform.Modules.HR;

[DependsOn(
    typeof(HRDomainModule),
    typeof(ERPPlatformEntityFrameworkCoreModule)
)]
public class HREntityFrameworkCoreModule : AbpModule
{
}
