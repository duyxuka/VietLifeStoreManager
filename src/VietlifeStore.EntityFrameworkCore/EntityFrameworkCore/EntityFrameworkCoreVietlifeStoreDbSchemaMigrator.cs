using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VietlifeStore.Data;
using Volo.Abp.DependencyInjection;

namespace VietlifeStore.EntityFrameworkCore;

public class EntityFrameworkCoreVietlifeStoreDbSchemaMigrator
    : IVietlifeStoreDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreVietlifeStoreDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the VietlifeStoreDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<VietlifeStoreDbContext>()
            .Database
            .MigrateAsync();
    }
}
