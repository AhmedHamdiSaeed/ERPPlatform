using Volo.Abp.Modularity;

namespace ERPPlatform;

/* Inherit from this class for your domain layer tests. */
public abstract class ERPPlatformDomainTestBase<TStartupModule> : ERPPlatformTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
