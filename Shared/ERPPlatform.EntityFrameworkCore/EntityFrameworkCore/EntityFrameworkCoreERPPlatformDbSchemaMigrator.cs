using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ERPPlatform.Data;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.EntityFrameworkCore;

public class EntityFrameworkCoreERPPlatformDbSchemaMigrator
    : IERPPlatformDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreERPPlatformDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the ERPPlatformDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<ERPPlatformDbContext>()
            .Database
            .MigrateAsync();
    }
}
