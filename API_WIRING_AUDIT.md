# Frontend ↔ API Wiring Audit

Date: 2026-09-01
Scope: every page and action in `angular/src/app` vs. the live backend API.

## Method

Swagger is the single source of truth — ABP's route naming cannot be inferred reliably.

1. Started the host and captured `https://localhost:44327/swagger/v1/swagger.json` (**263 paths**).
2. Extracted every route literal from the Angular app — the `core/services/api/*.ts` layer plus
   direct `HttpClient` calls in components — resolving each service's `apiPrefix()`
   (`/api/app`, `/api/hr`, `/api/inventory`, `/api/workflow`).
3. Normalized `${...}` and `{...}` placeholders and diffed method + path against Swagger.
4. Cross-checked all 64 backend `*AppService` classes against Swagger to catch services mounted at
   the wrong path, not just missing ones.
5. Smoke-tested the corrected endpoints against the live server.

**Result: 139 / 139 frontend call sites match the live API.**

## Bugs found and fixed

### Route mismatches (would 404 at runtime)

| Area | Frontend called | Actual route |
|---|---|---|
| Recruitment | `PUT candidate/{id}/update-stage` | `PUT /api/app/candidate/{id}/stage?newStage=` |
| HR attendance | `POST attendance/{id}/check-out` | `POST /api/hr/attendance/check-out/{attendanceId}` |
| HR attendance | `POST attendance/check-in?employeeId=` | `POST /api/hr/attendance/check-in/{employeeId}` |
| Sales | `POST sales-quotation/convert-to-invoice?quotationId=` | `POST /api/inventory/sales-quotation/convert-to-invoice/{quotationId}` |
| Auth | `POST /api/account/change-password` | `POST /api/account/my-profile/change-password` |

Root cause: a comment in `sales-api.service.ts` claimed *"ABP binds Guid parameters from query
string by convention"*. It does not — binding depends on the service signature. That comment was
removed; it was the likely source of the copy-pasted bugs.

### Silent data bug

`GET /api/hr/attendance?employeeId=` was **ignored by the server** — the base CRUD `GetListAsync`
had no such parameter, so the employee-detail page would have shown every employee's attendance
rows. Fixed by adding `AttendanceGetListInput { Guid? EmployeeId }` and overriding
`CreateFilteredQueryAsync` in `AttendanceAppService`. Verified live: 1 row for the owner, 0 for a
non-owner.

### Mis-mounted service

`IntegrationAppService` was mounted at the bare root `/api/app` instead of its own segment, so all
five calls from `integration-api.service.ts` to `/api/app/integration-config` failed. It also
squatted `GET/PUT/DELETE /api/app/{id}`. Renamed to `IntegrationConfigAppService`; it now serves
`/api/app/integration-config`, `/active-providers`, `/{id}`, `/{id}/test-connection`, and the
`/api/app` squat is gone.

### Unapplied migration

`GET /api/app/candidate` returned **500 `Invalid object name 'Candidates'`**. The migration
`20260831144100_AddRecruitmentExecutionHistoryReports` had been generated but never applied.
Fixed by running `Shared/ERPPlatform.DbMigrator`. Recruitment, workflow-execution-log,
report-definition and search all return 200 now.

### Error masking

`change-password.component.ts` swallowed failures with `catch { success.set(true) }` — the page
reported success even though the request 404'd. Replaced with real error surfacing.

## Correction to a previous pass

An earlier audit claimed three routes 404'd (`crm deal-stage`, `goods-receipt`, `plan features`).
**That was wrong** — all three exist and the frontend calls them correctly. They were mis-flagged by
inferring ABP conventions rather than reading Swagger. The genuine mismatches were the five above.

## Remaining: pages still rendering mock data

8 components render `MOCK_*` data. Backend services already exist for 7 of them, so these are
straightforward wiring tasks rather than backend work.

| Page | Mock symbols | Backend service available |
|---|---|---|
| `features/hr/attendance/attendance-list` | `MOCK_ATTENDANCE` | `HrApiService.getAttendance` |
| `features/hr/employees/employee-detail` | `MOCK_EMPLOYEES`, `MOCK_ATTENDANCE`, `MOCK_LEAVE_REQUESTS` | `HrApiService.getEmployee` |
| `features/hr/recruitment/recruitment-kanban` | `MOCK_CANDIDATES` | `RecruitmentApiService` |
| `features/notifications/notification-center` | `MOCK_NOTIFICATIONS` | `NotificationApiService` |
| `features/reports/report-center` | `MOCK_REPORTS` | `ReportsApiService` |
| `features/workflow/history/execution-history` | `MOCK_EXECUTION_LOGS` | `WorkflowExecutionApiService` |
| `features/inventory/transfers/stock-transfer-list` | `MOCK_PRODUCTS` | already uses `InventoryApiService` — needs a product lookup |
| `shared/components/barcode-scanner` | `MOCK_PRODUCTS` | needs a product-lookup endpoint |

## Verification

- `dotnet build ERPPlatform.slnx` → 0 errors
- `npx tsc -p tsconfig.app.json --noEmit` → clean
  (the `global-search-modal.component.ts` TS2345 introduced when `GlobalSearchService.search()`
  became async is resolved)
- Route diff: **139 / 139** call sites match
- Live smoke test: candidate, workflow-execution-log, report-definition, search,
  integration-config, attendance, notifications → all 200
- Attendance rows created during verification were deleted; the table is back to 0

## Not covered

- Manufacturing, Projects and Assets still have no backend at all (pre-existing gap, see
  `FEATURE_BACKLOG.md`).
- 246 Swagger endpoints are never called by the web frontend — expected: many serve the Flutter
  mobile app (`MOBILE_API_AUDIT.md`) or admin/identity surfaces.
