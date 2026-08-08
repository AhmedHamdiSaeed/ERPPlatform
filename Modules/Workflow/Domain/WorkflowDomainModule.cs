using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.Workflow;

[DependsOn(
    typeof(ERPPlatformDomainModule)
)]
public class WorkflowDomainModule : AbpModule
{
}
