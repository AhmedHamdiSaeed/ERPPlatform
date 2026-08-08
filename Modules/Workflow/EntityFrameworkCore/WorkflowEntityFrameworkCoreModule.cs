using Volo.Abp.Modularity;
using ERPPlatform.EntityFrameworkCore;

namespace ERPPlatform.Modules.Workflow;

[DependsOn(
    typeof(WorkflowDomainModule),
    typeof(ERPPlatformEntityFrameworkCoreModule)
)]
public class WorkflowEntityFrameworkCoreModule : AbpModule
{
}
