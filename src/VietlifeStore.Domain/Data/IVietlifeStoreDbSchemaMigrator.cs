using System.Threading.Tasks;

namespace VietlifeStore.Data;

public interface IVietlifeStoreDbSchemaMigrator
{
    Task MigrateAsync();
}
