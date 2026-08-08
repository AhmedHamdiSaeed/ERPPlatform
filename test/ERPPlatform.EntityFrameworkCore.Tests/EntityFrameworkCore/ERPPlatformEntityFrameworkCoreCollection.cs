using Xunit;

namespace ERPPlatform.EntityFrameworkCore;

[CollectionDefinition(ERPPlatformTestConsts.CollectionDefinitionName)]
public class ERPPlatformEntityFrameworkCoreCollection : ICollectionFixture<ERPPlatformEntityFrameworkCoreFixture>
{

}
