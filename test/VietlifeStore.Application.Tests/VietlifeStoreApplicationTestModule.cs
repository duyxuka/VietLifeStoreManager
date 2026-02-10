using Volo.Abp.Modularity;

namespace VietlifeStore;

[DependsOn(
    typeof(VietlifeStoreApplicationModule),
    typeof(VietlifeStoreDomainTestModule)
)]
public class VietlifeStoreApplicationTestModule : AbpModule
{

}
