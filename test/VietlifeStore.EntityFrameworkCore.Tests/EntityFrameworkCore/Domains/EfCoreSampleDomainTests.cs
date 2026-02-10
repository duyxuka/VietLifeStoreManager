using VietlifeStore.Samples;
using Xunit;

namespace VietlifeStore.EntityFrameworkCore.Domains;

[Collection(VietlifeStoreTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<VietlifeStoreEntityFrameworkCoreTestModule>
{

}
