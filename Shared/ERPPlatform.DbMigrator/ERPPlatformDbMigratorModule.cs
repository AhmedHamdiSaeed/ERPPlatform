using ERPPlatform.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ERPPlatform.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ERPPlatformEntityFrameworkCoreModule),
    typeof(ERPPlatformApplicationContractsModule)
    )]
public class ERPPlatformDbMigratorModule : AbpModule
{
}
