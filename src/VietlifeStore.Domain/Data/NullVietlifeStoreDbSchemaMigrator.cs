using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace VietlifeStore.Data;

/* This is used if database provider does't define
 * IVietlifeStoreDbSchemaMigrator implementation.
 */
public class NullVietlifeStoreDbSchemaMigrator : IVietlifeStoreDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
