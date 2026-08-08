using Volo.Abp.Modularity;

namespace ERPPlatform;

public abstract class ERPPlatformApplicationTestBase<TStartupModule> : ERPPlatformTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
