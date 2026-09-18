using Volo.Abp.Modularity;
using ERPPlatform.Modules.AI;
using ERPPlatform.Modules.HR;
using ERPPlatform.Modules.Inventory;
using ERPPlatform.Modules.Workflow;

namespace ERPPlatform;

[DependsOn(
    typeof(ERPPlatformApplicationModule),
    typeof(ERPPlatformDomainTestModule),
    typeof(AIApplicationModule),
    typeof(HRApplicationModule),
    typeof(InventoryApplicationModule),
    typeof(WorkflowApplicationModule)
)]
public class ERPPlatformApplicationTestModule : AbpModule
{

}
