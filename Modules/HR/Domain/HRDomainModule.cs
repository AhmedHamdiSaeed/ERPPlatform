using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.HR;

[DependsOn(
    typeof(ERPPlatformDomainModule)
)]
public class HRDomainModule : AbpModule
{
}
