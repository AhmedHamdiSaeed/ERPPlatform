using System.Threading.Tasks;

namespace ERPPlatform.Data;

public interface IERPPlatformDbSchemaMigrator
{
    Task MigrateAsync();
}
