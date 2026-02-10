using Volo.Abp.Modularity;

namespace VietlifeStore;

public abstract class VietlifeStoreApplicationTestBase<TStartupModule> : VietlifeStoreTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
