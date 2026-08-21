using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Uow;
using ERPPlatform.Modules.HR.Application;
using ERPPlatform.Modules.Inventory.Application;
using ERPPlatform.Modules.Workflow.Application;
using ERPPlatform.Modules.AI.Application;

namespace ERPPlatform.EntityFrameworkCore;

[DependsOn(
    typeof(ERPPlatformApplicationTestModule),
    typeof(ERPPlatformEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(HRApplicationModule),
    typeof(InventoryApplicationModule),
    typeof(WorkflowApplicationModule),
    typeof(AIApplicationModule)
    )]
public class ERPPlatformEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        Configure<SettingManagementOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = new SqliteConnection("Data Source=:memory:");
        _sqliteConnection.Open();

        var connectionString = _sqliteConnection.ConnectionString;

        // Create tables using the EF Core context
        var options = new DbContextOptionsBuilder<ERPPlatformDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        using (var dbContext = new ERPPlatformDbContext(options))
        {
            dbContext.Database.EnsureCreated();
        }

        services.Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
        });

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.UseSqlite(_sqliteConnection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }
}
