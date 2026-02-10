using Volo.Abp.Modularity;

namespace VietlifeStore;

[DependsOn(
    typeof(VietlifeStoreDomainModule),
    typeof(VietlifeStoreTestBaseModule)
)]
public class VietlifeStoreDomainTestModule : AbpModule
{

}
