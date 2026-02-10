using VietlifeStore.Samples;
using Xunit;

namespace VietlifeStore.EntityFrameworkCore.Applications;

[Collection(VietlifeStoreTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<VietlifeStoreEntityFrameworkCoreTestModule>
{

}
