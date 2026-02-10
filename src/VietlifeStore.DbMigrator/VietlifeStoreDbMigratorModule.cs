using VietlifeStore.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace VietlifeStore.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VietlifeStoreEntityFrameworkCoreModule),
    typeof(VietlifeStoreApplicationContractsModule)
)]
public class VietlifeStoreDbMigratorModule : AbpModule
{
}
