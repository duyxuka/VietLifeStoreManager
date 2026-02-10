using Xunit;

namespace VietlifeStore.EntityFrameworkCore;

[CollectionDefinition(VietlifeStoreTestConsts.CollectionDefinitionName)]
public class VietlifeStoreEntityFrameworkCoreCollection : ICollectionFixture<VietlifeStoreEntityFrameworkCoreFixture>
{

}
