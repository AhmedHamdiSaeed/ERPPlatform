using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ERPPlatform.Modules.Workflow;

[DependsOn(
    typeof(WorkflowDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class WorkflowApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<WorkflowApplicationModule>();
        });
    }
}
