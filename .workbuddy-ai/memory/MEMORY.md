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
