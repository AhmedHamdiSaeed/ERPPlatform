using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Workflow;

[DependsOn(
    typeof(WorkflowApplicationModule),
    typeof(ERPPlatformHttpApiModule)
)]
public class WorkflowHttpApiModule : AbpModule
{
}
