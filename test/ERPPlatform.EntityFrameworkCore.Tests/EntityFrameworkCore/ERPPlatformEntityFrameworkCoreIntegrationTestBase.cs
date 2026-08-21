using ERPPlatform.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace ERPPlatform;

public abstract class ERPPlatformEntityFrameworkCoreTestBase<TStartupModule>
    : ERPPlatformTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
