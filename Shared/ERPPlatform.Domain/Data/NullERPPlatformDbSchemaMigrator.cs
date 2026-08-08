using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Data;

/* This is used if database provider does't define
 * IERPPlatformDbSchemaMigrator implementation.
 */
public class NullERPPlatformDbSchemaMigrator : IERPPlatformDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
