using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace ERPPlatform.Modules.AI;

using ERPPlatform.Modules.AI.Application;
using ERPPlatform.Modules.AI.Application.Rag;

[DependsOn(
    typeof(AIDomainModule),
    typeof(ERPPlatformApplicationModule)
)]
public class AIApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<AiOptions>(configuration.GetSection("AI"));

        context.Services.AddHttpClient("AI", client =>
        {
            client.Timeout = System.TimeSpan.FromSeconds(60);
        });

        context.Services.AddTransient<IAiProvider, OpenAiCompatibleAiProvider>();
        context.Services.AddTransient<IRagRetriever, RagRetriever>();
    }
}
