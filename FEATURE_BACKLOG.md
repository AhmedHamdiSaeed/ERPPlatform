# ERPPlatform — Feature Backlog (ranked, with effort estimates)

> Generated 2026-08-31 from a fresh repo scan. Answers "what features can we add" and
> ranks them by leverage vs effort so the next build can be picked with confidence.
> Effort: **S** ≤ 0.5 day · **M** ~1–3 days · **L** ≥ 1 week (incl. migration/tests).

## Current state (verified this scan)

- **Build health:** `dotnet build ERPPlatform.slnx` → 0 errors / 0 warnings. `ng build` (dev) → clean.
- **Backend modules:** `AI`, `HR`, `Inventory`, `Workflow` + Shared app services
  (`Documents`, `Search`, `Reports`, `Payments`, `CRM`, `Finance`, `ApprovalCenter`,
  `Dashboards`, `LeavePolicy`). Note: **Manufacturing / Projects / Assets have NO backend** —
  they are frontend stub folders only.
- **Already delivered (recent):** real LLM AI assistant + natural-language workflow graph,
  API versioning, rate limiting, dashboard config + leave-policy/accrual engine,
  resumable chunked file import (frontend + backend) with Excel/CSV validation,
  3h desktop / 6-month mobile session + silent token refresh, mobile handoff docs.
- **Localization:** `en.json` = `ar.json` = 275 keys (matching key sets, full Arabic translation),
  RTL CSS baseline added, and the language choice persists across reloads via `localStorage`.
  **Chrome is externalized** — sidebar nav (~62 `Menu:` keys), header (search/import/AI/notifications/
  user-menu), and settings page (cards, labels, `NotifChannel:` keys) all route through the
  `abpLocalization` pipe. **HR module is externalized too** — all 9 HR components (employees, departments,
  positions, attendance, leave, contracts, employee-detail, payroll, recruitment) route titles, buttons,
  table headers, form labels, dialogs, status filters, and the employee-detail tabs through
  `abpLocalization`. Remaining: ~360 feature-page templates in other modules still have hardcoded English
  strings (see #16 for the ongoing module-by-module pass).
- **Test coverage:** ~8 real test classes vs ~48 AppServices (most `Sample*` files are ABP scaffolding).
- **Data model:** entities still **denormalized** — `string DepartmentName / WarehouseName /
  SupplierName / CustomerName` used in entities + several AppServices (no FK navigation).
- **Frontend mock data:** 8 components still import `core/mock/mock-data`
  (barcode-scanner, workflow history, report-center, notification-center, stock-transfer-list,
  recruitment-kanban, employee-detail, attendance-list).

## Tier 1 — highest leverage (build on what exists)

| # | Feature | Why now | Effort |
|---|---------|---------|--------|
| 1 | **RAG / context-aware AI over ERP data** ✅ IMPLEMENTED | `RagRetriever` queries Products/Employees/Invoices/Customers/Suppliers/Documents by keyword and injects matches into the LLM system prompt. Lexical retrieval (no vector store). | L |
| 2 | **Mobile offline sync + conflict resolution** | 6-mo session + refresh done, but no offline queue. Field staff (attendance, stock counts, deliveries) need store-and-forward. | L |
| 3 | **Payroll engine** | Have `Employee.LeaveBalance` + attendance GPS; no salary/wage/payslip/statutory deduction layer yet. | L |
| 4 | **Inventory depth: lots/bins/landed cost + FK refactor** | Valuation & traceability are weak while fields are denormalized strings. Converts `DepartmentName` etc. to real FKs. | L |
| 5 | **Arabic + RTL localization** ✅ IMPLEMENTED (baseline) | `ar.json` completed to 26 keys w/ Arabic translations; RTL CSS baseline (Cairo font + physical-side mirroring) added to `styles.scss`; language choice persisted via `localStorage` in `StateService`. Full UI-string externalization remains (#16). | M |

## Tier 2 — breadth / platform completeness

| # | Feature | Why now | Effort |
|---|---------|---------|--------|
| 6 | **Manufacturing module** (backend + frontend): BOM, work orders, routing | Currently a 1-file frontend stub; no backend. | L |
| 7 | **Projects module**: tasks, Gantt, billing | 1-file frontend stub; no backend. | L |
| 8 | **Assets module**: lifecycle, depreciation | 1-file frontend stub; no backend. | L |
| 9 | **Report designer** on top of `DashboardWidget` / `UserDashboardConfig` tables | Tables exist; no drag-drop builder yet. | L |
| 10 | **Persistent background jobs (Hangfire)** | `FileImportBackgroundJob` is still a `Task.Delay` stub; real parsing + scheduling needs a durable runner. | M |
| 11 | **Custom fields engine** (JSON column or EAV) | Customers always want to extend entities; cheap to add per-module once the pattern exists. | L |
| 12 | **Notification inbox UI** over the existing `NotificationHub` | Hub fires `ReceiveNotification` but there's no read-state inbox. | M |

## Tier 3 — cleanup / risk reduction (not new features)

| # | Item | Why now | Effort |
|---|------|---------|--------|
| 13 | **Replace mock data** in the 8 components with real API calls | Removes demo-data debt; makes those screens real. | M |
| 14 | **Consolidate the two SignalR services** (mock `core/services/signalr.service.ts` vs real `shared/services/signalr.service.ts`) | Removes a duplicate/confusing surface. | S |
| 15 | **Expand test coverage** (target HR/Inventory/Finance/AI AppServices) | ~8 real tests vs ~48 services; protects the features above as they grow. | M |
| 16 | **Full localization keys + RTL pass** 🟡 IN PROGRESS | Continuation of #5. Resource grew 26→143→275 keys (`UI:`, `Menu:`, `NotifChannel:`, `HR:` namespaces + tab labels). Chrome (sidebar/header/settings) externalized; HR module fully externalized (9 components). Remaining: ~360 feature-page templates in other modules still hardcoded (mechanical, module-by-module pass). | M |

## Suggested first move
Start with **#1 RAG AI** — it reuses the just-built LLM assistant and delivers the most
visible customer value, or **#5 Arabic+RTL** if the immediate market is MENA. Pick one and
the implementation can begin immediately.
