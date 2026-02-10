using Volo.Abp.Modularity;

namespace VietlifeStore;

/* Inherit from this class for your domain layer tests. */
public abstract class VietlifeStoreDomainTestBase<TStartupModule> : VietlifeStoreTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
