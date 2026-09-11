# ERPPlatform — Project Conventions

## Build & run
- Backend: `cd Host/ERPPlatform.HttpApi.Host && dotnet run --launch-profile "ERPPlatform.HttpApi.Host"` (https://localhost:44327). **Never `--no-launch-profile`** → binds :5000 + env=Production → `/api/abp/application-configuration` "0 Unknown Error".
- Angular prod: `npm run build:prod` (output `angular/dist/ERPPlatform`). Dev: `ng build --configuration development --no-delete-output-path` (CLI dist cleanup trips sandbox bulk-delete guard).
- `dotnet build` fails MSB3021/MSB3027 while host runs (locks its own DLLs) → build with `-o <temp>`. Stale host on 44327 → kill PID from MSB3027 / `netstat -ano | grep 44327`. CS2012 Access denied = stale Roslyn node → `dotnet build-server shutdown`.
- Sandbox blocks `tasklist/Get-Process/wmic/reg`; `ConvertTo-SecureString -AsPlainText` blocked.

## Prod deploy — SAME-ORIGIN SPA (current plan, since 2026-09-11)
- Serve SPA from `https://erpplatform.runasp.net` (same origin as API → no CORS, no redirect-URI, no Netlify SSL issues). `environment.prod.ts` reverted so `baseUrl/issuer/apis.default.url` all = `https://erpplatform.runasp.net`.
- .NET host must serve `wwwroot` SPA + fallback: `app.UseStaticFiles()` + `app.UseRouting()` + `endpoints.MapFallbackToFile("index.html")` (after `UseConfiguredEndpoints()`). Build Angular into `Host/ERPPlatform.HttpApi.Host/wwwroot`.
- Netlify experiment abandoned: all 3 sites returned 502 (account `ssl:false`, no API fix). Revoke PAT `nfp_DJVjJ9P7wj29DTD7yQUJEKYyvUeHCFz141c2` at app.netlify.com/user/applications.

## Prod config precedence & gotchas
- `AddAppSettingsSecretsJson()` appends `appsettings.secrets.json` **last** → overrides env/production/args. Never put `ConnectionStrings` there; use gitignored `appsettings.Production.json`.
- `web.config` sets no ASPNETCORE_ENVIRONMENT → IIS defaults to **Production** (loads appsettings.Production.json).
- Verify deploy: `dotnet publish -c Release -o C:/tmp/pubtestN` (fresh N), then `ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://localhost:5055 dotnet ERPPlatform.HttpApi.Host.dll` → `/` 302 /swagger, `/api/abp/application-configuration` 200, swagger.json 339 paths.
- WebDeploy not drivable from here — user must Publish in Visual Studio.

## OpenIddict (IIS 500 on every URL)
- Dev cert opens Windows store → `CryptographicException: Access denied` under app-pool identity. Fix in `PreConfigureServices` (non-Dev only): `AbpOpenIddictAspNetCoreOptions.AddDevelopmentEncryptionAndSigningCertificate=false` + `AddEncryptionCertificate`/`AddSigningCertificate` (real RSA cert). Cert: `Host/.../openiddict.pfx` RSA2048 20y, pass `Erp2026-OpenIddict-Pfx`, or base64 in `OpenIddict:CertificatePfxBase64` (preferred). Load with `X509KeyStorageFlags.MachineKeySet|EphemeralKeySet` via `X509CertificateLoader.LoadPkcs12`.

## HTTPS / origins
- `http://erpplatform.runasp.net` 307→https. All `App:SelfUrl/ClientUrl/CorsOrigins/RedirectAllowedUrls`, `AuthServer:Authority` must be **https**; `RequireHttpsMetadata=true`. `inprocess` forwards scheme — no `UseForwardedHeaders`. `DisableTransportSecurityRequirement()` self-disables under https.
- Redirect URIs seeded from DbMigrator `OpenIddict:Applications:ERPPlatform_App:RootUrl` + `AdditionalRedirectUris` (prod+dev). Running DbMigrator resets to RootUrl unless `AdditionalRedirectUris` present.

## DB / migrations
- `dotnet-ef` unavailable (no Design pkg) → `cd Shared/ERPPlatform.DbMigrator && dotnet run`. Unapplied migration → endpoints 500 `Invalid object name`.
- Split-horizon DB: server-side `db66804.databaseasp.net` (10.0.0.31), dev `db66804.public.databaseasp.net` (5.9.179.197). sqlcmd at `/c/Program Files/Microsoft SQL Server/Client SDK/ODBC/170/Tools/Binn/SQLCMD.EXE` (`-C`).

## Logins / tenants
- Multi-tenancy on. Tenants: `Acme, AlAmal, TechFlow` (host=TenantId NULL). Admin `admin`/`admin@abp.io`/`1q2w3E*` valid host + all tenants. `ERPPlatform_App` client: `gt:password`+`gt:refresh_token`, scope `offline_access ERPPlatform`. Tenant via `__tenant` header.
- Data issue: 4 duplicate rows in `AbpUsers` for several users — clean before mobile bind.

## Localization (en/ar)
- `angular/src/assets/i18n/{en,ar}.json`. `get(key)` returns key → missing Arabic = missing ar.json key. Mixed AR/EN text nodes untranslatable; split them.
