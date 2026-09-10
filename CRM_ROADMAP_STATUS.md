# CRM Roadmap — status against the 6-phase plan

> Scanned 2026-09-06 from the live repo. `✅` shipped · `🟡` partial · `❌` not started.
> The headline: **Phases 1–3 are mostly built on the backend but half-invisible in the UI**,
> and Phases 4–6 do not exist yet. The cheapest wins are finishing what is already coded.

---

## Phase 1 — Core CRM

| Feature | API | UI | Where it lives |
|---|---|---|---|
| Leads | ✅ | ✅ | `LeadAppService` (`Modules/HR/Application/OrgAppServices.cs`) · `/sales/crm/leads` |
| Lead sources | ✅ | 🟡 | `Lead.Source` free string + `CrmDashboardAppService` source stats; no managed lookup list |
| Contacts | ✅ | ✅ | `CrmContactAppService` (CRUD + `make-primary`); page at `/sales/crm/contacts` (2026-09-06) |
| Companies / Accounts | ✅ | ✅ | `Customer` entity (Industry, Website, Owner, VIP, Tags) + `CustomerAppService`; `/sales/customers` |
| Lead qualification | ✅ | ✅ | New → Contacted → Qualified → Unqualified, incl. lost reason (`MarkContacted` / `Qualify` / `MarkUnqualified`) |
| Opportunities | ✅ | 🟡 | `Deal` + `CrmAppService`; kanban exists at `/sales/pipeline`, no dedicated detail/form page |
| Sales pipeline | ✅ | ✅ | `DealStages`: New → Qualified → Proposal → Negotiation → Closed Won / Closed Lost, per-stage probability |
| Activities | ✅ | ✅ | `CrmActivityAppService` (call/meeting/task/email/follow-up, due date, priority, outcome, complete/reopen); page at `/sales/crm/activities` (2026-09-06) |
| Notes & attachments | ✅ | 🟡 | `CrmNoteAppService` (pin + one attachment name/url). Add/delete UI exists **on Customer 360 only** — not on leads or deals |
| Customer timeline | ✅ | ✅ | `Customer360AppService.Timeline` rendered in `/sales/crm/customer/:id` |
| Tags | 🟡 | ❌ | Comma-separated string on Lead / Deal / Customer. No tag entity, no tag filter UI |
| Search & filters | 🟡 | 🟡 | `LeadListInput` (status + keyword), activity + note filters. No unified CRM search |

## Phase 2 — Opportunity management

| Field / capability | Status | Note |
|---|---|---|
| Expected deal value | ✅ | `Deal.Value` |
| Probability | ✅ | `Deal.Probability`, auto-set per stage by `DealStages.ProbabilityFor` |
| Expected closing date | ✅ | `Deal.ExpectedCloseDate` |
| Sales owner | ✅ | `Deal.OwnerName` + `OwnerUserId` |
| Competitor | ✅ | `Deal.Competitor` |
| Lost reason | ✅ | `Deal.LostReason`, set by `MarkLost` |
| Products / services | ❌ | **No line items.** `Deal` has a single `Value` only |
| Price estimates | ❌ | Same gap — needs a `DealLineItem` entity |
| Opportunity notes | ✅ | `CrmNote.DealId` |
| Multiple contacts | ❌ | `Deal.ContactId` is single-valued; needs a Deal↔Contact join |
| Multiple activities | ✅ | `CrmActivity.DealId` |

## Phase 3 — ERP integration (the differentiator)

```
Lead ──ConvertToOpportunity──▶ Deal ──❌──▶ Quotation ──❌──▶ Sales Order ──❌──▶ Invoice ──✅──▶ Payment
```

| Link | Status | Note |
|---|---|---|
| Lead → Opportunity | ✅ | `LeadAppService.ConvertToOpportunityAsync` sets `ConvertedDealId` / `ConvertedCustomerId` |
| Opportunity → Quotation | ❌ | **Biggest break in the chain.** Nothing creates a quote from a won/negotiating deal |
| Quotation → Sales Order | ❌ | Missing |
| Quotation → Invoice | ✅ | `SalesQuotationAppService.ConvertToInvoiceAsync` |
| Sales Order → Invoice | ❌ | Missing |
| Invoice → Payment | ✅ | `PaymentAppService` (process / refund / capture) |

**Customer 360 — already shipped** (`GET /api/app/customer-360/{id}`, UI at `/sales/crm/customer/:id`):
contacts, opportunities, quotations, orders, invoices, payments, activities, notes, timeline +
roll-ups (pipeline value, order value, invoiced, outstanding, paid, open/overdue activities).
Not yet included: support tickets and workflow history — neither module exists.

## CRM dashboard

`CrmDashboardAppService.GetKpisAsync` computes **everything on your list** — total/new/qualified
leads, open opportunities, pipeline value, won/lost, conversion rate, win rate, average deal value,
average sales cycle, calls/meetings/tasks, overdue follow-ups, activities by owner, lead-source
breakdown, per-stage pipeline.

**Status: shipped 2026-09-06.** `/sales/crm/dashboard` now renders all of the above (KPI cards,
pipeline-by-stage bar, lead-source doughnut, activity KPIs, per-salesperson table, upcoming
activities). It is one request — `GET /api/app/crm-dashboard/kpis` — so the screen is a pure
renderer and cannot drift from the backend.

## Phase 4 — Automation

Workflow engine exists (`Modules/Workflow`), but there are **no CRM domain events and no CRM
triggers**. None of these fire today: lead created / assigned / qualified, opportunity created /
stage changed / won / lost, customer created / inactive, follow-up overdue.
Actions (create task, assign user, notify, email, change status, create activity) exist as
primitives in the workflow module but are not wired to CRM.

## Phase 5 — Marketing ❌
No `Campaign`, `CampaignMember`, lead list, email campaign, landing page or web-to-lead entity.

## Phase 6 — Customer service ❌
No ticket / SLA / priority / category / satisfaction entity. (`MaintenanceRequest` is asset
maintenance, not customer support.)

---

## Recommended order from here

**Batch A — make what exists visible (1–2 days, no new backend)** ✅ done 2026-09-06

1. ~~**CRM dashboard page** on `crm-dashboard/kpis`~~ → `crm-dashboard.component.*`
2. ~~**Contacts page** (list + create/edit, link to account/lead, primary flag)~~ → `contacts.component.*`
3. ~~**Activities page** (calls/meetings/tasks, overdue filter, complete/reopen inline)~~ → `activities.component.*`
4. **Notes panel** on lead / deal (still missing — Customer 360 has one)
5. ~~Sidebar entries + permission catalog + `en`/`ar` keys~~ (101 new keys in both dictionaries)

**Batch B — close the ERP chain (3–5 days, 1 migration)**

6. `DealLineItem` entity → products/services + price estimate on opportunities; roll up to `Deal.Value`.
7. `ConvertDealToQuotationAsync` → carry line items into a `SalesQuotation`.
8. `ConvertQuotationToOrderAsync` and `ConvertOrderToInvoiceAsync`.
9. `DealContact` join so an opportunity can have multiple contacts.

**Batch C — differentiation (1 week+)**

10. `Tag` entity replacing CSV strings + tag filter across CRM grids.
11. CRM workflow triggers (domain events → existing workflow engine) with the 10 triggers / 8 actions
    from Phase 4.
12. Unified CRM search across leads, contacts, accounts, opportunities, activities.

**Later:** Marketing (Phase 5), Customer service (Phase 6), lead scoring, forecasting.

---

## Placement note (technical debt worth knowing)

CRM code is split across three places, which makes it hard to find:

- `Shared/ERPPlatform.Application/Crm/` — contacts, activities, notes, dashboard, customer 360
- `Shared/ERPPlatform.Application/Crm/CrmAppService.cs` — deals + pipeline stages
- `Modules/HR/Application/OrgAppServices.cs` — **leads** and **customers** (a 500-line file that also
  holds companies, branches, suppliers, projects, assets and payroll)

Leads and customers sitting under the HR module is an accident of history. Consolidating them into
`Shared/ERPPlatform.Application/Crm/` would be a safe, mechanical move and should happen before
Phase 5 adds more surface area.
