using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ERPPlatform.EntityFrameworkCore;
using ERPPlatform.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Microsoft.OpenApi.Models;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Asp.Versioning;

using ERPPlatform.Modules.HR;
using ERPPlatform.Modules.Inventory;
using ERPPlatform.Modules.Workflow;
using ERPPlatform.Modules.AI;
using ERPPlatform.Dapper.Queries;
using ERPPlatform.Hubs;

namespace ERPPlatform;

[DependsOn(
    typeof(ERPPlatformHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(ERPPlatformApplicationModule),
    typeof(ERPPlatformEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(HRHttpApiModule),
    typeof(HREntityFrameworkCoreModule),
    typeof(InventoryHttpApiModule),
    typeof(InventoryEntityFrameworkCoreModule),
    typeof(WorkflowHttpApiModule),
    typeof(WorkflowEntityFrameworkCoreModule),
    typeof(AIHttpApiModule),
    typeof(AIEntityFrameworkCoreModule),
    typeof(ERPPlatformDapperQueriesModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(AbpBlobStoringFileSystemModule)
)]
public class ERPPlatformHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("ERPPlatform");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureAuthentication(context);
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers(context);

        // ABP does not automatically register the OpenIddict ASP.NET Core
        // assembly as an MVC application part in this host, so the /connect/*
        // endpoints (token, authorize, logout, userinfo) are never mapped and
        // password-grant logins fail with 405. Register the assembly explicitly.
        context.Services.AddControllers()
            .AddApplicationPart(typeof(Volo.Abp.OpenIddict.AbpOpenIddictAspNetCoreModule).Assembly);

        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureBlobStoring(context);
        ConfigureRateLimiting(context);
        ConfigureTokenLifetimes(context);

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });

        Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.Conventions.Add(new RouteNormalizationConvention());
        });

        // Register custom SignalR user ID provider for user-targeted push
        context.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, AbpUserIdProvider>();
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureBlobStoring(ServiceConfigurationContext context)
    {
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                container.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = Path.Combine(
                        context.Services.GetHostingEnvironment().ContentRootPath,
                        "wwwroot", "blobs");
                });
            });
        });
    }

    /// <summary>
    /// Access tokens stay short lived on purpose. The long user session
    /// (3 hours on desktop, 6 months on phones/tablets) is owned by the client
    /// and survives because the SPA silently exchanges its refresh token for a
    /// new access token. The refresh token therefore has to outlive the longest
    /// session we support, i.e. 180 days.
    /// </summary>
    private void ConfigureTokenLifetimes(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        var accessTokenMinutes = configuration.GetValue<int?>("Auth:AccessTokenLifetimeMinutes") ?? 30;
        var refreshTokenDays = configuration.GetValue<int?>("Auth:RefreshTokenLifetimeDays") ?? 180;

        Configure<OpenIddictServerOptions>(options =>
        {
            options.AccessTokenLifetime = TimeSpan.FromMinutes(accessTokenMinutes);
            options.RefreshTokenLifetime = TimeSpan.FromDays(refreshTokenDays);
        });
    }

    private void ConfigureRateLimiting(ServiceConfigurationContext context)
    {
        context.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(httpContext =>
            {
                var userId = httpContext.User?.FindFirst("sub")?.Value
                             ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                             ?? httpContext.Connection.RemoteIpAddress?.ToString()
                             ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 200,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            // More restrictive limit for write operations (POST/PUT/DELETE)
            options.AddPolicy("WriteOperations", httpContext =>
            {
                var method = httpContext.Request.Method.ToUpperInvariant();
                if (method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH")
                {
                    var userId = httpContext.User?.FindFirst("sub")?.Value
                                 ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                 ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
                }

                return RateLimitPartition.GetNoLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous");
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    """{"error":{"code":"RATE_LIMITED","message":"Too many requests. Please slow down."}}""",
                    cancellationToken);
            };
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ERPPlatformDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}ERPPlatform.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<ERPPlatformDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}ERPPlatform.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<ERPPlatformApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}ERPPlatform.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<ERPPlatformApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}ERPPlatform.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ERPPlatformApplicationModule).Assembly, opts =>
            {
                opts.RootPath = "app";
            });
            options.ConventionalControllers.Create(typeof(HRApplicationModule).Assembly, opts =>
            {
                opts.RootPath = "hr";
            });
            options.ConventionalControllers.Create(typeof(InventoryApplicationModule).Assembly, opts =>
            {
                opts.RootPath = "inventory";
            });
            options.ConventionalControllers.Create(typeof(WorkflowApplicationModule).Assembly, opts =>
            {
                opts.RootPath = "workflow";
            });
            options.ConventionalControllers.Create(typeof(AIApplicationModule).Assembly, opts =>
            {
                opts.RootPath = "ai";
            });
        });

        // Configure API Versioning using Asp.Versioning.Mvc directly
        context.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("api-version"));
        });

        context.Services.AddMvcCore().AddApiExplorer();
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"ERPPlatform", "ERPPlatform API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ERPPlatform API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName ?? type.Name);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERPPlatform API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            c.OAuthScopes("ERPPlatform");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}

public class RouteNormalizationConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel?.Template != null)
                {
                    while (selector.AttributeRouteModel.Template.Contains("//"))
                    {
                        selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template.Replace("//", "/");
                    }
                }
            }

            foreach (var action in controller.Actions)
            {
                foreach (var selector in action.Selectors)
                {
                    if (selector.AttributeRouteModel?.Template != null)
                    {
                        while (selector.AttributeRouteModel.Template.Contains("//"))
                        {
                            selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template.Replace("//", "/");
                        }
                    }
                }
            }
        }
    }
}
