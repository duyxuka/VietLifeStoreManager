using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VietlifeStore.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class VietlifeStoreDbContextFactory : IDesignTimeDbContextFactory<VietlifeStoreDbContext>
{
    public VietlifeStoreDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        VietlifeStoreEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<VietlifeStoreDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new VietlifeStoreDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../VietlifeStore.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
