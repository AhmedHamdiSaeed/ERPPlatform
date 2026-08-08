using Volo.Abp.Modularity;
using ERPPlatform.EntityFrameworkCore;

namespace ERPPlatform.Dapper.Queries;

[DependsOn(
    typeof(ERPPlatformEntityFrameworkCoreModule)
)]
public class ERPPlatformDapperQueriesModule : AbpModule
{
}
