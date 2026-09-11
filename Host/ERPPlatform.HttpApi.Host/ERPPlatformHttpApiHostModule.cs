using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Asp.Versioning;
using ERPPlatform.Application.Imports;
using ERPPlatform.Dapper.Queries;
using ERPPlatform.Domain.Imports;
using ERPPlatform.EntityFrameworkCore;
using ERPPlatform.Hubs;
using ERPPlatform.Imports;
using ERPPlatform.Modules.AI;
using ERPPlatform.Modules.HR;
using ERPPlatform.Modules.Inventory;
using ERPPlatform.Modules.Workflow;
using ERPPlatform.MultiTenancy;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

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
    // Pass phrase of Host/ERPPlatform.HttpApi.Host/openiddict.pfx and of the base64 copy in
    // appsettings.Production.json. Both must match or the certificate fails to load.
    private const string OpenIddictCertificatePassPhrase = "Erp2026-OpenIddict-Pfx";

    // File name of the production certificate inside the content root. Only used when
    // OpenIddict:CertificatePfxBase64 is not configured.
    private const string OpenIddictCertificateFileName = "openiddict.pfx";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("ERPPlatform");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        // ABP registers a *development* encryption + signing certificate by default, and that code
        // path calls X509Store.Open(...) to persist/read a user-scoped certificate. On IIS the app
        // pool identity has no Windows user profile, so X509Store.Open throws
        // "CryptographicException: Access is denied". Because OpenIddict resolves its credentials
        // lazily from OpenIddictValidationServerIntegrationConfiguration.Configure(...), the throw
        // escapes from AuthenticationMiddleware - which runs on EVERY request - so every URL,
        // /swagger/index.html included, returns a 500. It never happens on a dev machine because
        // the developer's own account can open the store.
        //
        // Outside Development we therefore disable the development certificate and supply a real
        // one. See https://abp.io/docs/latest/deployment/configuring-openIddict
        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                var certificate = LoadOpenIddictCertificate(context.Services);

                serverBuilder.AddEncryptionCertificate(certificate);
                serverBuilder.AddSigningCertificate(certificate);

                // OpenIddict refuses to issue tokens over plain http unless told otherwise:
                //   error "invalid_request" - "This server only accepts HTTPS requests." (ID2083)
                // erpplatform.runasp.net now has a TLS binding, so this branch is inactive there -
                // it is kept as a safety net for any http-only deployment (staging, or running the
                // Release build locally). It only trips when App:SelfUrl really is "http://", so
                // switching to HTTPS automatically restores the requirement.
                var configuration = context.Services.GetConfiguration();
                var publicUrl = configuration["App:SelfUrl"] ?? configuration["AuthServer:Authority"];
                if (!string.IsNullOrEmpty(publicUrl) &&
                    publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    serverBuilder.UseAspNetCore().DisableTransportSecurityRequirement();
                }
            });
        }
    }

    /// <summary>
    /// Loads the production OpenIddict certificate. Prefers the base64 copy embedded in
    /// configuration (OpenIddict:CertificatePfxBase64) because on shared hosting a loose .pfx file
    /// in the content root can be blocked by ACLs or reported as "file not found"; falls back to
    /// openiddict.pfx on disk.
    /// </summary>
    private static X509Certificate2 LoadOpenIddictCertificate(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        var environment = services.GetHostingEnvironment();

        var passPhrase = configuration["OpenIddict:CertificatePassPhrase"]
                         ?? OpenIddictCertificatePassPhrase;

        byte[] bytes;
        var base64 = configuration["OpenIddict:CertificatePfxBase64"];
        if (!string.IsNullOrWhiteSpace(base64))
        {
            bytes = Convert.FromBase64String(base64.Trim());
        }
        else
        {
            var path = Path.Combine(environment.ContentRootPath, OpenIddictCertificateFileName);
            bytes = File.ReadAllBytes(path);
        }

        // MachineKeySet | EphemeralKeySet keeps the private key out of any user profile, which is
        // exactly what an IIS app pool without "Load User Profile" needs.
        return X509CertificateLoader.LoadPkcs12(
            bytes,
            passPhrase,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
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
        ConfigureEmployeeImport(context, configuration);
        ConfigureHangfire(context, configuration);

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

    /// <summary>
    /// Binds the <c>EmployeeImport</c> config section and registers the Host-side
    /// implementations of the application-layer abstractions (scheduler -> Hangfire,
    /// notifier -> SignalR). The maintenance hosted service self-registers via
    /// <see cref="Volo.Abp.DependencyInjection.ISingletonDependency"/>.
    /// </summary>
    private void ConfigureEmployeeImport(ServiceConfigurationContext context, IConfiguration configuration)
    {
        Configure<EmployeeImportOptions>(configuration.GetSection("EmployeeImport"));

        context.Services.AddTransient<IEmployeeImportJobScheduler, EmployeeImportHangfireScheduler>();
        context.Services.AddTransient<IEmployeeImportNotifier, EmployeeImportSignalRNotifier>();
        context.Services.AddTransient<IEmployeeImportScheduleArgsFactory, EmployeeImportScheduleArgsFactory>();
    }

    /// <summary>
    /// Hangfire backing store + worker server. The worker only drains the dedicated
    /// <c>employee-import</c> queue (plus <c>default</c>), so import work is isolated
    /// from any other background processing and vice-versa. The dashboard is exposed
    /// only in Development; in production it must sit behind authenticated/app-authorised
    /// middleware.
    /// </summary>
    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        context.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        context.Services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "employee-import", "default" };
            // Keep a single worker: the dev DB is SQL Server LocalDB, which shares one
            // connection pool with EF. Multiple Hangfire workers were exhausting that pool
            // (SqlException "max pool size was reached" / Named Pipes 40/53/64), starving
            // every web request. One worker is plenty for the employee-import queue in dev.
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}:employee-import";
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
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();

        // On shared hosting the app's own log file is not reachable, so a 500 arrives as a blank
        // page with no clue. Setting "DetailedErrors": true in appsettings.Production.json switches
        // on the full stack trace in the browser for a deployed environment.
        // MUST BE REGISTERED FIRST: ABP's UseErrorPage() is added further down and can only catch
        // exceptions thrown after it, so anything failing earlier would otherwise surface as an
        // empty 500 with no audit-log entry.
        // Keep it false in production - it exposes stack and source details publicly.
        if (env.IsDevelopment() || configuration.GetValue<bool>("DetailedErrors", false))
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();

        // Serve the Angular SPA at the site root "/". ABP's Swagger wiring registers a redirect from
        // "/" to "/swagger", which would bounce first-time visitors away from the app. Rewrite the bare
        // root path to index.html (served from wwwroot by MapAbpStaticAssets below) so the SPA loads
        // instead. Only "/" is affected; "/swagger" and every API route are untouched.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Request.Path = "/index.html";
            }

            await next();
        });

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

        // Enabled by default - Swagger is this host's API documentation. On a public deployment it
        // also publishes the full endpoint list to anyone who finds the URL, so set
        // "Swagger": { "Enabled": false } in appsettings.Production.json to switch it off without
        // a rebuild. Both the JSON document and the UI are gated together, so a disabled Swagger
        // returns 404 instead of rendering an empty page.
        if (configuration.GetValue<bool>("Swagger:Enabled", true))
        {
            app.UseSwagger(options =>
            {
                // This host *is* the OpenIddict server, so /connect/authorize and /connect/token
                // always live on whatever origin the Swagger page is being served from.
                // `AuthServer:Authority` in appsettings.json is a localhost default; leaving it
                // baked into the document means a deployed Swagger page sends the browser back to
                // the developer's machine and "Authorize" fails silently. Deriving the URLs from the
                // live request keeps Swagger working unmodified on localhost, in Docker, and behind
                // any reverse proxy or IIS application alias.
                options.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    if (swaggerDoc.Components?.SecuritySchemes is null)
                    {
                        return;
                    }

                    var baseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";

                    foreach (var scheme in swaggerDoc.Components.SecuritySchemes.Values)
                    {
                        if (scheme.Type != SecuritySchemeType.OAuth2 || scheme.Flows?.AuthorizationCode is null)
                        {
                            continue;
                        }

                        scheme.Flows.AuthorizationCode.AuthorizationUrl =
                            new Uri($"{baseUrl}/connect/authorize", UriKind.Absolute);
                        scheme.Flows.AuthorizationCode.TokenUrl =
                            new Uri($"{baseUrl}/connect/token", UriKind.Absolute);
                    }
                });
            });

            app.UseAbpSwaggerUI(c =>
            {
                // Relative on purpose: an absolute "/swagger/..." breaks when the app is deployed
                // under an IIS application alias or a reverse-proxy sub-path.
                c.SwaggerEndpoint("v1/swagger.json", "ERPPlatform API");

                c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                c.OAuthScopes("ERPPlatform");
            });
        }

        if (env.IsDevelopment())
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAllowAllFilter() }
            });
        }

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();

        // Serve the Angular SPA from THIS same origin. MapAbpStaticAssets() above already serves the
        // physical files out of wwwroot (index.html, main.*.js, assets/...). This fallback rewrites any
        // unmatched request (the root "/" and client-side routes like /dashboard) to index.html so deep
        // links and refreshes work instead of 404-ing. It is the lowest-priority endpoint, so /api/*,
        // /connect/* and every ABP controller still win. Build the Angular app into wwwroot.
        // MapFallbackToFile is an extension on IEndpointRouteBuilder (not IApplicationBuilder), so it
        // must be registered inside UseEndpoints.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapFallbackToFile("index.html");
        });
    }
}

/// <summary>
/// Development-only, unauthenticated access to the Hangfire dashboard. This is NOT
/// safe for production — replace with a real policy/role check before exposing the
/// dashboard on a public endpoint.
/// </summary>
public class HangfireDashboardAllowAllFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
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
