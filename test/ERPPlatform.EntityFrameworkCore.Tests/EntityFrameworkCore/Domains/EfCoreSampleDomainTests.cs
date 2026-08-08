using ERPPlatform.Samples;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Domains;

[Collection(ERPPlatformTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ERPPlatformEntityFrameworkCoreTestModule>
{

}
