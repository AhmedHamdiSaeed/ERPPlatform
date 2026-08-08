using ERPPlatform.Samples;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications;

[Collection(ERPPlatformTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ERPPlatformEntityFrameworkCoreTestModule>
{

}
