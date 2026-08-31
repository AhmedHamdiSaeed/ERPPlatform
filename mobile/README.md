# ERPPlatform — Mobile App

> **Yes: the Flutter app lives inside this repository**, at `mobile/app/`.
> This repo is already a monorepo (`.NET` host + modules, Angular web app, Dapper queries,
> shared design tokens), and the mobile app needs to stay in sync with all of them.

```
ERPPlatform/
├── Host/         .NET API host
├── Modules/      .NET business modules
├── Shared/       domain + EF Core
├── angular/      web app
└── mobile/
    ├── design-system/   ← shared visual contract (you consume this)
    └── app/             ← your Flutter project goes here
```

## 1. Create the project

Flutter is **not** installed on the main dev machine, so run this on yours:

```bash
cd mobile
flutter create app --org com.erpplatform --project-name erp_mobile --platforms android,ios
```

Then commit from `mobile/app`. The repo `.gitignore` already covers `.dart_tool/`, `build/`,
`.flutter-plugins`, and `build_runner` output (`*.g.dart`, `*.freezed.dart`, …).

**Scope rule:** you work under `mobile/**`. Changes to API contracts are fine — just flag them so
the backend side can be updated in the same pull request.

## 2. Design system (single source of truth)

| File | What it is |
|---|---|
| `design-system/erp_theme.dart` | Flutter `ThemeData` (light + dark) + `ErpColors`, `ErpRadius`, `ErpShadows` — drop-in |
| `design-system/design-tokens.json` | Machine-readable tokens, regenerate tooling can read this |
| `design-system/DESIGN_SYSTEM.md` | Full visual reference (typography, spacing, component specs) |
| `design-system/reference-styles.scss` | The original Angular source these were extracted from |

Usage — copy `erp_theme.dart` into your project and wire it up:

```bash
cp design-system/erp_theme.dart app/lib/theme/erp_theme.dart
```

```dart
import 'theme/erp_theme.dart';

MaterialApp(
  theme: erpLightTheme,
  darkTheme: erpDarkTheme,
  ...
);
```

The file exports `erpLightTheme` / `erpDarkTheme` (`ThemeData`) plus the `ErpColors`, `ErpRadius`
and `ErpShadows` helpers for one-off styling. It only imports `package:flutter/material.dart`,
so there is nothing else to install. Font is **Inter** (add `google_fonts` and use
`GoogleFonts.inter()`).

Keep the copy in sync: if the web theme changes, re-extract and update this folder first.

## 3. API & auth contract

Base URL (local dev): `https://localhost:44327`
- Android emulator → `https://10.0.2.2:44327`
- iOS simulator → `https://localhost:44327`
- Physical device → your machine's LAN IP

The API only serves HTTPS with a self-signed dev certificate, so on Android you need a
`network_security_config.xml` that trusts it (or run the host with an extra HTTP Kestrel URL).

Full endpoint inventory: see **`MOBILE_API_AUDIT.md`** at the repo root.

### Login

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=ERPPlatform_App
&username=<email>&password=<password>
&scope=offline_access%20ERPPlatform
```

Response contains `access_token` **and** `refresh_token` (the `ERPPlatform_App` client is seeded
with the `refresh_token` grant, and `offline_access` is requested — both are required to get it).

### Session rules (implemented Aug 2026)

| | Lifetime |
|---|---|
| Mobile session (your app) | **6 months / 180 days**, absolute from login |
| Web session (desktop browser) | 3 hours |
| Server access token | 30 minutes |
| Server refresh token | 180 days |

- Sessions are **absolute**, not idle-based — activity does not extend them.
- Store tokens in **`flutter_secure_storage`**, never `SharedPreferences`.
- The access token dies after ~30 min. On a `401`:
  1. `POST /connect/token` with `grant_type=refresh_token&client_id=ERPPlatform_App&refresh_token=<refresh_token>`
  2. save the new tokens, **retry the failed request once** (never loop)
  3. if the refresh fails → clear storage and send the user to the login screen, returning them
     to the screen they were on after they sign in again
- Logout → `POST /connect/revocation` with `token=<refresh_token>`, `token_type_hint=refresh_token`,
  `client_id=ERPPlatform_App`, then clear local storage.
- Send `Authorization: Bearer <access_token>` on every API call. The backend is multi-tenant, so
  include the `__tenant` header when a tenant is resolved by header rather than by subdomain.

## 4. Suggested structure inside `app/`

```
lib/
├── core/        api client, auth service, secure storage, router, DI
├── theme/       erp_theme.dart (copied from design-system/)
├── shared/      reusable widgets matching the web components
└── features/    dashboard, hr, inventory, sales, finance, …
```

## 5. CI

Add a GitHub Actions job that runs only on `mobile/**` paths:

```yaml
on:
  push:
    paths: ['mobile/**']
jobs:
  mobile:
    steps:
      - uses: subosito/flutter-action@v2
      - run: flutter pub get && flutter analyze && flutter test
        working-directory: mobile/app
```

## 6. Running the backend & frontend locally (so you can use the API)

You don't need the web app to build the mobile app, but you **do** need the API running — the
mobile app talks to it directly. Run these once on your machine.

### Prerequisites
- **.NET 9 SDK** (the host targets `net9.0`)
- **Node 20.11+** (for the Angular web app)
- **Flutter** (only if you're building `mobile/app` itself)
- A **PostgreSQL / SQL Server** instance matching the `ConnectionStrings` in the host's
  `appsettings.json` (ask the backend team for the dev connection string).

### 6.1 One-time: create the database
```bash
dotnet run --project Shared/ERPPlatform.DbMigrator
```
This applies all EF Core migrations and seeds initial data (including the `admin` user and the
`ERPPlatform_App` OAuth client the mobile app logs in with). Re-run it whenever new migrations are
added to the repo.

### 6.2 Start the API (ASP.NET Core host)
```bash
dotnet run --project Host/ERPPlatform.HttpApi.Host
```
- Listens on **`https://localhost:44327`** (HTTPS, self-signed dev certificate).
- Keep this terminal open / run it as a **long-lived background process** — the API must stay up
  while you develop and test the mobile app.
- First run restores NuGet packages and may take a minute.
- Swagger UI is available at `https://localhost:44327/swagger` — handy for exploring endpoints
  without the app.
- Default dev admin: **`admin` / `1q2w3E*`** (use a real seeded user for mobile login testing).

> Environment is `Development` automatically via `launchSettings.json`. To override:
> `ASPNETCORE_ENVIRONMENT=Development dotnet run --project Host/ERPPlatform.HttpApi.Host`.

### 6.3 (Optional) Start the Angular web app
```bash
cd angular
npm install
npm start          # = ng serve --open  → http://localhost:4200
```
`angular/src/environments/environment.ts` already points `apiUrl` at `https://localhost:44327`,
so the web app calls the same API the mobile app uses. `http://localhost:4200` is whitelisted in
`App:CorsOrigins`, so web↔API calls just work.

### 6.4 Point the mobile app at the API
Use the base URL from §3:
| Target | Base URL |
|---|---|
| Android emulator | `https://10.0.2.2:44327` |
| iOS simulator | `https://localhost:44327` |
| Physical device | `https://<your-machine-LAN-IP>:44327` |

- **CORS**: native Dart HTTP clients (`dart:io`/`http`) don't enforce CORS, so normal mobile calls
  are fine. If you use a WebView or see CORS errors, add your origin to `App:CorsOrigins` in
  `Host/ERPPlatform.HttpApi.Host/appsettings.json` (currently `http://localhost:4200` is allowed).
- **Android self-signed cert**: the API uses HTTPS with a dev cert, so add a
  `network_security_config.xml` that trusts it (see §3). On iOS the simulator generally accepts
  `localhost` with the dev cert.

### Best practices
1. **Run the API before the app**, and leave it running the whole dev session.
2. **Re-run the DbMigrator** after pulling changes that include new migrations.
3. One API instance serves **both** the web app and the mobile app — no need to run two.
4. Use the `ERPPlatform_App` client + `offline_access` scope exactly as documented in §3; the
   6-month mobile session depends on the refresh token being returned.
5. Don't commit local connection strings or the `openiddict.pfx` dev certificate.

## 7. Running the mobile app

Prerequisite: the **API must be running** (§6.2) and Flutter must be installed on your machine.
The app project lives in `mobile/app` (create it first — see §1 if it doesn't exist yet).

```bash
cd mobile/app
flutter pub get
flutter run            # pick a simulator / emulator / device when prompted
```

Before the first run:
1. **Copy the design system** (§2): `cp ../design-system/erp_theme.dart lib/theme/erp_theme.dart`.
2. **Point at the API** (§6.4):
   - Android emulator → `https://10.0.2.2:44327`
   - iOS simulator → `https://localhost:44327`
   - Physical device → `https://<your-machine-LAN-IP>:44327`
3. **Android self-signed cert**: add a `network_security_config.xml` that trusts the dev
   certificate (§3 / §6.4), otherwise HTTPS calls fail with a cert error.

Login with a seeded user (default dev admin is `admin` / `1q2w3E*`). The 6-month mobile session,
refresh-token handling, and `IMPORT_DONE` notifications are all exercised against the running API.

## 8. Full local startup checklist

Bring the whole platform up in this order (one terminal per long-lived process):

| # | What | Command | URL / note |
|---|------|---------|------------|
| 1 | **Seed the database** (once, re-run after new migrations) | `dotnet run --project Shared/ERPPlatform.DbMigrator` | creates tables + seeds `admin` & `ERPPlatform_App` |
| 2 | **API host** (keep running) | `dotnet run --project Host/ERPPlatform.HttpApi.Host` | `https://localhost:44327` |
| 3 | **Angular web** (optional) | `cd angular && npm install && npm start` | `http://localhost:4200` |
| 4 | **Mobile app** (optional) | `cd mobile/app && flutter pub get && flutter run` | emulator/simulator/device |

**Verify:**
- API: open `https://localhost:44327/swagger` — you should see the Swagger UI.
- Web: log in at `http://localhost:4200` with the seeded user.
- Mobile: log in with the same user; theImport button (header) starts a resumable upload
  that the API processes in the background and confirms via a real-time notification.

**Ports at a glance:** API `44327` (HTTPS) · Web `4200` (HTTP) · Mobile emulator `10.0.2.2:44327`.
A single API instance serves **both** the web app and the mobile app — you don't need two.

> Best practice: always start the API (step 2) first and keep it alive for the whole dev session;
> the web and mobile apps are just clients talking to it. Re-run the DbMigrator (step 1) whenever
> you pull changes that include new EF Core migrations.

