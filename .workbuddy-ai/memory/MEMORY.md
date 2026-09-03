# ERPPlatform — Project Conventions

## Session & Authentication

| Scope | Lifetime |
|---|---|
| Desktop / web browser | 3 hours (absolute, from login) |
| Phone / tablet (incl. mobile browser + PWA) | 6 months = 180 days |
| Server access token (OpenIddict) | 30 minutes |
| Server refresh token (OpenIddict) | 180 days |

- Sessions are **absolute**, not idle-based: activity does not extend them, and after expiry any
  action (click, navigation, API call) forces a redirect to `/auth/login?returnUrl=<page>`.
  After re-login the user returns to that exact page (query params included).
- The long mobile session works because the SPA silently exchanges its refresh token for a new
  access token; the refresh token lifetime must stay >= the longest client session (180 days).
- Config knobs live in `Host/.../appsettings.json` under `Auth:` and are read by
  `ConfigureTokenLifetimes()` in `ERPPlatformHttpApiHostModule`.
- Dev testing hook: append `?sessionSeconds=20` to the login URL to shrink the session
  (ignored in production builds).

## Build verification gotchas

- Angular: `ng build --configuration development --no-delete-output-path` — the CLI's cleanup of
  `dist` trips the sandbox bulk-delete guard.
- .NET: `dotnet build` fails with MSB3021/MSB3027 while `ERPPlatform.HttpApi.Host` is running
  (locks its own DLLs). Build with `-o <temp dir>` to verify compilation, then restart the host.
- **Pitfall:** `dotnet run --no-build` after `dotnet build -o <tempdir>` silently runs **stale**
  binaries from `bin/` — the `-o` build never updates `bin/`. To run fresh code, stop the host and
  let `dotnet run` build itself (no `-o`, no `--no-build`).
- A stale host keeps serving an old Swagger doc. Before re-verifying routes, free the port:
  `netstat -ano | grep 44327` → `Stop-Process -Id <pid> -Force`.
- `wmic.exe` is blocked by the sandbox — use `netstat`/`tasklist` or PowerShell instead.

## IIS Express port-squat on 44327 (silent login hangs)

- **`iisexpress.exe` left over from a Visual Studio debug session can hold port 44327.**
  Symptom: `dotnet run` succeeds but Kestrel silently fails to bind (look for it in the background
  task — no "Now listening on https://localhost:44327" line). Angular keeps hitting whatever is
  on the port (IIS Express), not Kestrel.
- User-visible symptom: `token` and `application-configuration` return 200 through the IIS proxy,
  but the protected `/api/app/notification/notifications` (and other API routes) hang forever as
  "pending" in DevTools → Network → the SPA is stuck on "Authenticating…" / "session expired".
- Fix: `tasklist | findstr iis` → `taskkill /PID <iisexpress_pid> /F` (and tray), then
  `cd Host/ERPPlatform.HttpApi.Host && dotnet run`. Confirm Kestrel with
  `curl -skI https://localhost:44327/swagger/v1/swagger.json` — must show `Server: Kestrel`.
- Detect quickly: if port 44327 is held but `tasklist` shows no `ERPPlatform.HttpApi.Host`,
  something else (IIS Express, ServiceHub, debugger proxy) owns it.

## Database / migrations

- `dotnet-ef` refuses to run because `ERPPlatform.HttpApi.Host` does not reference
  `Microsoft.EntityFrameworkCore.Design`. Use the migrator app instead:
  `cd Shared/ERPPlatform.DbMigrator && dotnet run`.
- A generated migration is not applied until the DbMigrator runs. Symptom of an unapplied
  migration: routes and services look fine but endpoints 500 with `Invalid object name '<Table>'`.

## ABP routing conventions (verified against live Swagger)

- Conventional controllers are registered in `ConfigureConventionalControllers()`; root paths are
  `app` (Shared assembly), `hr`, `inventory`, `workflow`, `ai`. All entities/services live in the
  Shared assembly regardless of which module folder they sit in.
- ABP strips the HTTP-verb prefix from the method name: `UpdateStageAsync` → `/stage`,
  `GetActiveProvidersAsync` → `/active-providers`, `PassQualityCheckAsync` → `/pass-quality-check`.
- **Do not assume parameters bind from the query string.** In ABP 9.3 conventional controllers,
  simple-type params (`id`, `conversationId`, `otherUserId`, `userId`, `messageId`) are placed in the
  **path**, not the query. Guessing query binding has caused repeated 404s
  (e.g. `check-in/{employeeId}`, `check-out/{attendanceId}`, `convert-to-invoice/{quotationId}`,
  `start-direct/{otherUserId}`, `history/{conversationId}`, `toggle-reaction/{messageId}` are path
  params; `candidate/{id}/stage` uses `?newStage=` in the query). Always read Swagger.
- **Verb-inference quirk:** `Create/Add`→POST, `Update`→PUT, `Remove`→DELETE, `Get`→GET — but
  **`Edit*` maps to POST, not PUT**. `EditMessageAsync` → `POST /chat-message/{id}/edit-message`.
- **Application layer has no `Microsoft.AspNetCore.Mvc` reference.** You cannot put
  `[FromQuery]`/`[FromRoute]` on app-service params (build fails CS0234). Keep backend binding as
  ABP defaults and align the frontend service instead.
- A service can end up mounted at the bare root (`/api/app`) if its controller segment is dropped —
  renaming `IntegrationAppService` → `IntegrationConfigAppService` restored the expected
  `/api/app/integration-config`. Always confirm the actual route in Swagger.
- API versioning is query/header based (`QueryStringApiVersionReader` + `HeaderApiVersionReader`),
  so versioning never changes URLs.
- In ABP 9 the CRUD filtering hook is `CreateFilteredQueryAsync` (not `CreateFilteredQuery`).
  To add a filter, declare a DTO deriving from `PagedAndSortedResultRequestDto`, use it as the
  4th generic argument of `CrudAppService<,,TGetListInput,>`, and override that hook.

## Verifying frontend ↔ API wiring

Swagger is the source of truth. Workflow:
1. Start the host, fetch `https://localhost:44327/swagger/v1/swagger.json` (263 paths).
2. Diff every route literal in `angular/src/app` against it (scripts: `C:\tmp\route_diff.py`,
   `C:\tmp\svc_check.py`). Normalize `${...}` and `{...}` to a single placeholder before comparing.
3. Cross-check all `*AppService` classes against Swagger to catch services mounted at the wrong
   path. Ignore `ERPPlatformAppService` — it is abstract and intentionally not exposed.
