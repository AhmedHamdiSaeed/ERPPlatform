# Next module after CRM — Payroll vs Inventory depth

> Scanned 2026-09-06. `✅` shipped · `🟡` partial · `❌` missing.
> Both roadmaps were scored against what is actually in the repo, not against the UI demo.

## STATUS (updated 2026-09-06) — Payroll Sprint 1 DONE
- **Backend:** `SalaryComponent`/`EmployeeSalaryComponent`/`PayslipLine`/`PayrollAnomaly` entities,
  `PayrollCalculator` shared engine (preview == pay), `GetPreviewAsync` (no writes), 9 anomaly kinds
  (+`ZeroNet`), status machine Draft→…→Finalized with locking. Migration `20260906173308_AddCrmPhase1AndPayrollComponents`
  applied; endpoints smoke-tested on 44327.
- **UI:** `payroll-management` (simulation + Issues-to-Review + runs + payslips) at `/hr/payroll`,
  `salary-components` rule builder at `/hr/payroll/components`. `payroll-api.service.ts` added.
- **Demo seed:** 2 components (HOUSING 10% of BASIC, TAX 10% of BASIC) created; preview exits
  fallback and computes net 255,000 across 5 active employees.
- **Not yet done:** EmployeeSalaryComponent UI, payslip line breakdown in the slip modal (lines aren't
  returned by `GET payslips` — only by preview), ESS portal, GL/posting, bank-file export,
  approval workflow integration (the status machine is manual for now).

## Verdict

**Payroll second. Inventory depth third.** One structural finding drives this:

- **Payroll is additive.** The inputs are already captured — `Employee.Salary`,
  `Attendance.WorkingHours`, `Attendance.OvertimeHours`, `Employee.LeaveBalance`, plus a real
  leave-accrual engine. Nothing in the current model is *wrong*; you are adding a component
  model, a rule engine and a run pipeline on top of data you already have.
- **Inventory is a foundation rebuild, not a feature add.** Your entire quantity model is
  `Product.Stock` — a single `int` — with `Product.WarehouseName` as a *string*. There is no
  `StockMovement`/ledger entity anywhere, and **no unit cost field at all** (`Product.Price` is
  the selling price). Every one of the seven priorities below is impossible until
  `Product → Location → Quantity → Movement` exists.

That does not make inventory optional — it makes it *bigger*. And any inventory number shown
today is a guess, which is a correctness problem the moment a trading customer signs up.

---

## Payroll — current state

| Area | Status | What exists / what's missing |
|---|---|---|
| Salary & payroll | 🟡 | `Employee.Salary` only. `PayrollAppService.ProcessPayrollRunAsync` sums salaries, applies a flat **15%** deduction, and never writes a `Payslip`. No allowances, deductions, bonus, overtime, commission, loans or retro |
| Payroll rules engine | ❌ | No `SalaryComponent`, no formulas, no effective dates |
| Attendance | 🟡 | Import + GPS check-in/out exist. Late/early rules ❌, shift definitions ❌, night shift ❌, missing-punch handling ❌. `OvertimeHours` is captured but **never read by payroll** |
| Leave | 🟡 | Annual/sick/unpaid types, requests, and a real `LeavePolicy` accrual engine. Carry-forward ❌, encashment ❌, payroll integration ❌ |
| Payslips | 🟡 | `Payslip` entity + read endpoint. No generation, PDF, delivery, history UI or templates |
| Banking | ❌ | No bank file, no transfer batches, no payment status |
| Employee self-service | ❌ | Only `/profile`. No payslip/salary/leave-balance portal |
| Reports & analytics | ❌ | None beyond the payroll run totals |
| Taxes & compliance | ❌ | `TaxConfig` exists but is generic VAT. No social insurance, no government reports |
| Loans & advances | ❌ | — |
| Benefits | ❌ | — |
| Accounting | 🟡 | `Account` + `JournalEntry` entities exist and are unused. No payroll→GL mapping |
| Security | 🟡 | ABP permissions + `AuditLogEntry` exist. No payroll locking, no approval chain |
| Automation | ❌ | `FileImportBackgroundJob` is still a `Task.Delay` stub (no Hangfire runner) |

**Your five priorities:** simulation/preview ❌ · anomaly detection ❌ · flexible components ❌ ·
approval workflow ❌ (but the **Workflow module already exists** to build it on) · self-service ❌.

### Payroll sprint 1 (~1 week, 1 migration)
1. `SalaryComponent` (earning/deduction, fixed/percentage/formula, taxable, recurring, effective
   from/to, GL account, cost center) + `SalaryStructure` + `EmployeeSalaryAssignment`.
2. `PayrollPeriod` + rewrite `ProcessPayrollRunAsync` to generate real `Payslip` rows per employee
   from components + attendance + leave.
3. **Preview/simulation** — dry-run that returns gross, allowances, overtime, bonuses, deductions,
   taxes, net, employer cost, and a delta vs last period. No writes.
4. **Anomaly detection** — every rule you listed is computable from data on hand today: >40% salary
   jump, unusual overtime, negative net, missing bank account, duplicate employee, unexpected
   deduction, terminated-but-paid, missing attendance.
5. Approval states Draft → Calculated → HR Review → Finance Review → Finalized, with reversals
   instead of edits once finalized.

Leave the rule *builder* UI, banking files, loans, benefits and ESS for sprint 2. The differentiator
is simulation + anomaly detection, and neither needs a formula-builder UI to be useful.

---

## Inventory — current state

| Area | Status | What exists / what's missing |
|---|---|---|
| Product master | 🟡 | Sku, Barcode, Name, Category, Price, Stock, ReorderLevel, Unit. No variants, brands, UoM conversion, images, dimensions, weight |
| Barcodes | 🟡 | `barcode-scanner` component exists but still runs on **mock data** |
| Multi-warehouse | 🟡 | `Warehouse` entity + `StockTransfer`. Warehouse is a **denormalised string** on Product; no zones/aisles/racks/bins; no per-warehouse quantities |
| Stock levels | ❌ | One `int Stock` per product. No on-hand / available / reserved / committed / damaged / in-transit |
| Stock transfers | 🟡 | CRUD only. No approval, no bin-to-bin, no in-transit tracking |
| Goods receiving | 🟡 | `GoodsReceipt` + GRN + QC status. No partial, over/under or damaged handling |
| Goods issue | ❌ | `DeliveryNote` exists but is not stock-consuming |
| Stock adjustments | ❌ | No reason codes, no approval, no audit trail — stock is edited directly |
| Serial numbers | ❌ | — |
| Batch / lot | ❌ | — |
| Expiry / FEFO | ❌ | — |
| Forecasting | ❌ | — |
| Reorder | 🟡 | `ReorderLevel` field exists, nothing consumes it |
| Procurement | 🟡 | PO / PR / RFQ exist. No supplier lead times, no backorders |
| **Valuation** | ❌ | **No cost field at all.** FIFO / weighted average / standard cost are all impossible today |
| Landed costs | ❌ | — |
| Stock counts | 🟡 | `StockCount` + `StockCountItem` + app service. No freeze/snapshot, no blind count, no variance approval |
| Reservations | ❌ | — |
| Picking & packing | ✅ | `PickList`/`PackList` + items, real and wired |
| Returns | ❌ | — |
| Kits / bundles | ❌ | — |
| Analytics | 🟡 | Dashboard widgets exist; no turnover, dead stock, aging or shrinkage |
| Controls | 🟡 | ABP permissions. No stock locking |
| Multi-currency | 🟡 | `Currency` entity exists; purchase cost is single-currency |

**Your seven priorities:** ledger ❌ · availability engine ❌ · warehouse locations ❌ · batch +
expiry ❌ · counting workflow 🟡 (entities exist, no freeze/approval) · replenishment ❌ ·
dashboard ❌.

### Inventory foundation sprint (~1.5 weeks, 1 migration — do this before any inventory feature)
1. `StockLevel` (Product × Warehouse × optional bin): on-hand, reserved, available, incoming,
   min/max. Retire `Product.Stock` and `Product.WarehouseName`.
2. `StockMovement` — the ledger. Date, user, document reference, warehouse, bin, quantity,
   unit cost, total value, balance before/after. **Every** receipt, issue, transfer, adjustment,
   count variance and return writes one.
3. `Product.CostPrice` + valuation method (weighted average first — cheapest correct option).
4. Then, in order: locations (zone → rack → shelf), batch/expiry + FEFO, counting workflow with
   freeze and variance approval, replenishment suggestions, dashboard.

---

## Sequencing

```
CRM Batch B  →  Payroll sprint 1  →  Inventory foundation  →  Payroll sprint 2 / inventory features
(1 migration)   (1 migration)        (1 migration)
```

Payroll goes first because it is additive, demoable in a week, and monetises the module you have
already invested most in. Inventory's foundation is the single highest-value piece of *debt* in the
repo, but it cannot be demoed as a feature — so it should be the very next thing after payroll, and
not deferred behind payroll sprint 2.

**Exception:** if your first paying customers are traders, distributors or anyone holding expiring
stock, invert the order. Wrong stock value poisons finance reporting, and no amount of CRM or
payroll polish fixes that.

## What to skip for now

Projects, Assets and Manufacturing are frontend stubs with **zero backend** (I checked — only DTO
mapping entries in `HRApplicationAutoMapperProfile`). Manufacturing is only worth it once the
inventory foundation exists, since BOM consumption is just another movement type.
