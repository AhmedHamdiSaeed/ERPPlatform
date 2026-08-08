using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ERPPlatform.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class ERPPlatformDbContextFactory : IDesignTimeDbContextFactory<ERPPlatformDbContext>
{
    public ERPPlatformDbContext CreateDbContext(string[] args)
    {
        ERPPlatformEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<ERPPlatformDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new ERPPlatformDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ERPPlatform.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
