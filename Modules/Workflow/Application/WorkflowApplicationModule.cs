using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Workflow;

[DependsOn(
    typeof(WorkflowDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class WorkflowApplicationModule : AbpModule
{
}
