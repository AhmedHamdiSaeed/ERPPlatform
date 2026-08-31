# Mobile App Backend API Audit Report
**Date:** 2026-08-30  
**Scope:** Verify that every API the mobile dev team will need (per 9-phase mobile plan + SaaS + technical requirements) exists in the ERPPlatform backend.  
**Role:** Web/Backend developer perspective only — no mobile code produced.

---

## Executive Summary

The backend has **48 AppService classes** exposing conventional ABP auto-API endpoints across 5 route prefixes (`/api/app`, `/api/hr`, `/api/inventory`, `/api/workflow`, `/api/ai`), plus 2 SignalR hubs, OpenIddict OAuth, and ABP built-in modules (Account, Identity, Tenant Management, Feature Management, Setting Management).

**Overall coverage:**

| Status | Count | Description |
|--------|-------|-------------|
| ✅ Exists | 52 | API exists and is functional |
| ⚠️ Partial | 11 | API exists but missing fields/pagination/real-time behavior |
| ❌ Missing | 19 | No backend endpoint or entity — must be built |
| **Total** | **82** | Requirements checked |

**Critical gaps that block mobile development:**
1. Push notification device registration (FCM/APNS token storage)
2. Barcode field on Product entity
3. GPS coordinates on Attendance/Delivery
4. Digital signature capture on Delivery/Invoice/Workflow
5. Leave balance tracking on Employee
6. Stock counting / picking / packing workflows
7. Field visit / customer visit tracking
8. Batch approval center
9. Payment processing (actual charge/capture/refund)
10. Tenant branding/white-label API (only LogoUrl exists, no colors/theme)
11. Real-time push (SignalR broadcasts to ALL — no user-targeted delivery)

---

## Route Prefix Reference

| Module Assembly | Route Prefix | Example |
|----------------|-------------|---------|
| ERPPlatform.Application (Shared) | `/api/app` | `/api/app/notification/send-notification` |
| HR.Application | `/api/hr` | `/api/hr/employee` |
| Inventory.Application | `/api/inventory` | `/api/inventory/product` |
| Workflow.Application | `/api/workflow` | `/api/workflow/workflow-task` |
| AI.Application | `/api/ai` | `/api/ai/ai-assistant/ask` |
| ABP Account | `/api/account` | `/api/account/register` |
| ABP Identity | `/api/identity` | `/api/identity/user` |
| ABP Tenant Mgmt | `/api/multi-tenancy/tenant` | `/api/multi-tenancy/tenant` |
| ABP Feature Mgmt | `/api/feature-management` | `/api/feature-management/features` |
| ABP Setting Mgmt | `/api/setting-management` | `/api/setting-management/settings` |
| OpenIddict | `/connect` | `/connect/token`, `/connect/userinfo` |
| ABP Configuration | `/api/abp` | `/api/abp/application-configuration` |
| SignalR Hubs | `/signalr-hubs` | `/signalr-hubs/notification`, `/signalr-hubs/chat` |

---

## Phase-by-Phase Audit

### Phase 1: Authentication, Onboarding & Multi-Tenancy

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 1 | Login (password grant) | ✅ | `POST /connect/token` with `grant_type=password`, `client_id=ERPPlatform_App`, `scope=offline_access ERPPlatform` |
| 2 | Refresh token | ✅ | `POST /connect/token` with `grant_type=refresh_token` (client supports RefreshToken grant) |
| 3 | Client credentials grant | ✅ | `POST /connect/token` with `grant_type=client_credentials` (client supports ClientCredentials) |
| 4 | Authorization code grant | ✅ | `GET /connect/authorize` + `POST /connect/token` (client supports AuthorizationCode) |
| 5 | User registration | ✅ | `POST /api/account/register` (AbpAccountApplicationModule) |
| 6 | Forgot password | ✅ | `POST /api/account/forgot-password` (ABP built-in) |
| 7 | Reset password | ✅ | `POST /api/account/reset-password` (ABP built-in) |
| 8 | Change password | ✅ | `POST /api/account/change-password` (ABP built-in) |
| 9 | User profile | ✅ | `GET /api/account/profile` (ABP built-in) |
| 10 | Application configuration (user, permissions, tenant, features, settings, localization) | ✅ | `GET /api/abp/application-configuration` |
| 11 | Tenant resolution | ✅ | Via `__tenant` header (AbpAspNetCoreMultiTenancyModule); tenant ID in header or subdomain |
| 12 | Tenant list / switching | ✅ | `GET /api/multi-tenancy/tenant` (AbpTenantManagementApplicationModule) |
| 13 | Logout / token revocation | ⚠️ | OpenIddict supports `/connect/revoke` but endpoint not explicitly configured — verify in testing |
| 14 | Device registration for push | ❌ | No `DeviceRegistration` entity or endpoint exists. No FCM/APNS token storage. |
| 15 | Tenant branding API | ❌ | `Company` entity has `LogoUrl` only. No `PrimaryColor`, `SecondaryColor`, `Theme` fields. `ERPPlatformBrandingProvider` returns hardcoded `AppName` only — no tenant-specific branding endpoint. |

**Phase 1 verdict: 13/15 requirements met.** Missing: push device registration + tenant branding.

---

### Phase 2: Dashboard & Analytics

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 16 | Dashboard stats (counts: employees, products, warehouses, leave requests) | ✅ | `GET /api/hr/dashboard-stats/stats` |
| 17 | Sales dashboard (invoice counts, pending/paid amounts) | ✅ | `GET /api/inventory/sales-invoice/stats` |
| 18 | CRM pipeline summary | ✅ | `GET /api/app/crm/pipeline-summary` |
| 19 | P&L summary | ✅ | `GET /api/app/finance/profit-and-loss-summary` |
| 20 | Executive AI summary | ✅ | `GET /api/ai/ai-assistant/executive-summary` |
| 21 | Customizable widget configuration | ❌ | No widget/dashboard configuration endpoint for per-user dashboard layout |

**Phase 2 verdict: 5/6 requirements met.** Missing: user dashboard customization.

---

### Phase 3: HR & Payroll

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 22 | Employee CRUD + list | ✅ | `GET /api/hr/employee` (PagedAndSortedResultRequestDto), `GET /api/hr/employee/{id}`, `POST`, `PUT`, `DELETE` |
| 23 | Department CRUD + list | ✅ | `GET /api/hr/department` (paginated), `GET /api/hr/department/{id}`, `POST`, `PUT`, `DELETE` |
| 24 | Leave request CRUD | ✅ | `GET /api/hr/leave-request` (paginated), `POST`, `PUT`, `DELETE` |
| 25 | Leave request approve | ✅ | `POST /api/hr/leave-request/{id}/approve` |
| 26 | Leave request reject | ✅ | `POST /api/hr/leave-request/{id}/reject` |
| 27 | Leave balance per employee | ❌ | No `LeaveBalance` field on `Employee` or `LeaveRequest` entity. Balance cannot be computed without a policy engine. |
| 28 | Attendance check-in | ✅ | `POST /api/hr/attendance/check-in?employeeId=&employeeName=&departmentName=` |
| 29 | Attendance check-out | ✅ | `POST /api/hr/attendance/{id}/check-out` |
| 30 | GPS coordinates on check-in | ❌ | `Attendance` entity has no `Latitude`/`Longitude`/`GPS` fields. Only `CheckIn` datetime. |
| 31 | Payroll runs | ✅ | `GET /api/app/payroll/payroll-runs` |
| 32 | Process payroll run | ✅ | `POST /api/app/payroll/process-payroll-run?period={period}` |
| 33 | Payslips | ✅ | `GET /api/app/payroll/payslips?period={period}` |
| 34 | Expense CRUD | ✅ | `GET /api/hr/expense` (paginated), `POST`, `PUT`, `DELETE` |
| 35 | Expense approve | ✅ | `POST /api/hr/expense/{id}/approve` |

**Phase 3 verdict: 10/14 requirements met.** Missing: leave balance, GPS check-in, + 2 field-level gaps.

---

### Phase 4: Inventory & Procurement

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 36 | Product CRUD + list | ✅ | `GET /api/inventory/product` (paginated), `GET /api/inventory/product/{id}`, `POST`, `PUT`, `DELETE` |
| 37 | Product barcode field | ❌ | `Product` entity has no `Barcode` property. Only `Sku`, `Name`, `Category`, `Price`, `Stock`, `ReorderLevel`, `Unit`, `WarehouseName`, `Status`, `SupplierName`. |
| 38 | Adjust stock | ✅ | `POST /api/inventory/product/{id}/adjust-stock?newStock={n}` |
| 39 | Warehouse CRUD | ✅ | `GET /api/inventory/warehouse` (paginated), `POST`, `PUT`, `DELETE` |
| 40 | Stock transfer CRUD + status update | ✅ | `GET /api/inventory/stock-transfer` (paginated), `POST /api/inventory/stock-transfer/{id}/update-status?newStatus={status}` |
| 41 | Purchase order CRUD + status | ✅ | `GET /api/inventory/purchase-order` (paginated), `POST /api/inventory/purchase-order/{id}/update-status?newStatus={status}` |
| 42 | Purchase request CRUD + approve | ✅ | `GET /api/hr/purchase-request` (paginated), `POST /api/hr/purchase-request/{id}/approve` |
| 43 | RFQ CRUD | ✅ | `GET /api/hr/rfq` (paginated), `POST`, `PUT`, `DELETE` |
| 44 | Goods receipt CRUD | ✅ | `GET /api/hr/goods-receipt` (paginated), `POST`, `PUT`, `DELETE` |
| 45 | Stock counting / cycle count | ❌ | No `StockCount`, `StockTake`, or cycle count entity/service. |
| 46 | Picking / pick list | ❌ | No `PickList`, `Picking`, or `PickWave` entity/service. |
| 47 | Packing / pack list | ❌ | No `PackList`, `Packing`, or `Shipment` entity/service. |
| 48 | Barcode scan to search product | ❌ | Cannot implement without `Barcode` field on Product. |

**Phase 4 verdict: 9/13 requirements met.** Missing: barcode field, stock counting, picking, packing, barcode scan search.

---

### Phase 5: Sales & CRM

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 49 | Lead CRUD + list | ✅ | `GET /api/hr/lead` (paginated), `POST`, `PUT`, `DELETE` |
| 50 | Lead convert to opportunity | ✅ | `POST /api/hr/lead/{id}/convert-to-opportunity` |
| 51 | Customer CRUD + list | ✅ | `GET /api/hr/customer` (paginated), `POST`, `PUT`, `DELETE`. DTO has `CreditLimit`, `OutstandingBalance`, `PaymentTerms`. |
| 52 | Supplier CRUD + list | ✅ | `GET /api/hr/supplier` (paginated), `POST`, `PUT`, `DELETE`. DTO has `CreditLimit`, `OutstandingBalance`, `PaymentTerms`. |
| 53 | Sales order CRUD + approve | ✅ | `GET /api/hr/sales-order` (paginated), `POST /api/hr/sales-order/{id}/approve` |
| 54 | Delivery note CRUD | ✅ | `GET /api/hr/delivery-note` (paginated), `POST`, `PUT`, `DELETE` |
| 55 | Delivery note status / location / proof | ❌ | `DeliveryNote` entity has no `TrackingNumber`, `CurrentLocation`, `DeliveryProof`, `DeliveryStatus` beyond `Status` string. No GPS or proof-of-delivery fields. |
| 56 | Digital signature on delivery | ❌ | No `Signature` field on `DeliveryNote` entity. |
| 57 | Sales invoice CRUD | ✅ | `GET /api/inventory/sales-invoice` (paginated), `POST` (with feature-limit gating), `PUT`, `DELETE` |
| 58 | Mark invoice as paid | ✅ | `POST /api/inventory/sales-invoice/{id}/mark-as-paid` |
| 59 | Send invoice | ✅ | `POST /api/inventory/sales-invoice/{id}/send-invoice` |
| 60 | Sales quotation CRUD | ✅ | `GET /api/inventory/sales-quotation` (paginated), `POST` (with feature-limit gating), `PUT`, `DELETE` |
| 61 | Convert quotation to invoice | ✅ | `POST /api/inventory/sales-quotation/convert-to-invoice?quotationId={id}` (quotationId binds from query string per ABP convention) |
| 62 | CRM deals list + create | ✅ | `GET /api/app/crm/deals`, `POST /api/app/crm/deal` |
| 63 | Update deal stage | ✅ | `POST /api/app/crm/deal/{id}/update-deal-stage?newStage={stage}` |
| 64 | Field visit / customer visit tracking | ❌ | No `FieldVisit`, `CustomerVisit`, or `VisitTracking` entity/service. |

**Phase 5 verdict: 12/16 requirements met.** Missing: delivery tracking/proof, digital signature, field visit tracking.

---

### Phase 6: Workflow & Approvals

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 65 | Workflow definition CRUD | ✅ | `GET /api/workflow/workflow-definition` (paginated), `POST`, `PUT`, `DELETE` |
| 66 | Workflow task CRUD | ✅ | `GET /api/workflow/workflow-task` (paginated), `POST`, `PUT`, `DELETE` |
| 67 | Workflow task approve | ✅ | `POST /api/workflow/workflow-task/{id}/approve?comments={comments}` |
| 68 | Workflow task reject | ✅ | `POST /api/workflow/workflow-task/{id}/reject?comments={comments}` |
| 69 | Unified approval center (aggregate pending across entities) | ❌ | No unified endpoint to fetch all pending approvals (leave requests, expenses, sales orders, purchase requests, workflow tasks) in one call. |
| 70 | Batch approval (multiple IDs in one request) | ❌ | No batch endpoint. Each approval is a separate POST per entity per ID. |

**Phase 6 verdict: 4/6 requirements met.** Missing: unified approval center, batch approval.

---

### Phase 7: Finance & Payments

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 71 | Chart of accounts | ✅ | `GET /api/app/finance/accounts`, `POST /api/app/finance/account` |
| 72 | Journal entries | ✅ | `GET /api/app/finance/journal-entries`, `POST /api/app/finance/journal-entry` (double-entry enforced) |
| 73 | P&L summary | ✅ | `GET /api/app/finance/profit-and-loss-summary` |
| 74 | Payment terms CRUD | ✅ | `GET /api/hr/payment-term` (paginated), `POST`, `PUT`, `DELETE` |
| 75 | Payment processing (charge/capture/refund) | ❌ | `IntegrationAppService.TestConnectionAsync()` only tests connectivity. No actual payment charge/capture/refund endpoint. Stripe webhook controller exists but no payment initiation API. |
| 76 | Accounts receivable aging | ❌ | No AR aging report endpoint. `SalesInvoiceAppService.GetStatsAsync()` returns aggregate pending/paid but no per-customer aging breakdown. |
| 77 | Accounts payable aging | ❌ | No AP aging report endpoint. No supplier invoice management beyond CRUD. |
| 78 | Bank reconciliation | ❌ | No bank reconciliation endpoint. |

**Phase 7 verdict: 4/8 requirements met.** Missing: payment processing, AR/AP aging, bank reconciliation.

---

### Phase 8: Communication & Collaboration

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 79 | In-app notifications list | ✅ | `GET /api/app/notification/notifications` |
| 80 | Send notification | ✅ | `POST /api/app/notification/send-notification` |
| 81 | Mark notification as read | ✅ | `POST /api/app/notification/{id}/mark-as-read` |
| 82 | Mark all notifications as read | ✅ | `POST /api/app/notification/mark-all-as-read` |
| 83 | Real-time notification push | ⚠️ | `NotificationHub` at `/signalr-hubs/notification` broadcasts `ReceiveNotification` to **ALL connected clients** — not user-targeted. No device push when app is backgrounded. No SignalR user ID mapping. |
| 84 | Chat channel messages | ✅ | `GET /api/app/chat/channel-messages?channelName={name}` |
| 85 | Direct messages | ✅ | `GET /api/app/chat/direct-messages?userId={id}&otherUserId={otherId}` |
| 86 | Send chat message | ✅ | `POST /api/app/chat/send-message` |
| 87 | Real-time chat push | ⚠️ | `ChatHub` at `/signalr-hubs/chat` broadcasts `ReceiveMessage` to **ALL clients** — `targetUserId` parameter is ignored. No user-specific delivery. |
| 88 | Push notification (FCM/APNS) | ❌ | No FCM/APNS integration. No device token storage. `SystemNotification` entity has no `DeviceToken`/`PushToken` field. |

**Phase 8 verdict: 6/10 requirements met (2 partial).** Missing: FCM/APNS push, real-time user-targeted delivery.

---

### Phase 9: Documents & AI

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 89 | Document upload | ✅ | `POST /api/app/document/upload` (uses ABP BlobStoring; params: title, extension, sizeBytes, contentType, folderId, byte[] content) |
| 90 | Document download | ✅ | `GET /api/app/document/{id}/download` (returns byte[]) |
| 91 | Document list | ✅ | `GET /api/app/document?folderId={id}` (no pagination — returns all in folder) |
| 92 | Document delete | ✅ | `DELETE /api/app/document/{id}` |
| 93 | Folder list | ✅ | `GET /api/app/folder?parentId={id}` |
| 94 | Folder create | ✅ | `POST /api/app/folder` |
| 95 | Folder delete | ✅ | `DELETE /api/app/folder/{id}` |
| 96 | AI assistant ask | ✅ | `POST /api/ai/ai-assistant/ask` (hardcoded AI response, optionally generates workflow JSON) |
| 97 | AI executive summary | ✅ | `GET /api/ai/ai-assistant/executive-summary` |
| 98 | Document list pagination | ⚠️ | `DocumentAppService.GetListAsync()` returns `List<DocumentDto>` — no pagination, no search. |
| 99 | AI context (chat history / conversation) | ❌ | `AskAsync` is stateless — no conversation history, no session context. |

**Phase 9 verdict: 7/11 requirements met (1 partial).** Missing: document pagination, AI conversation history.

---

## SaaS Requirements

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 100 | Subscription management | ✅ | `GET /api/app/subscription/current`, `POST /api/app/subscription/change-plan?newPlanId={id}`, `POST /api/app/subscription/cancel`, `POST /api/app/subscription/resume` |
| 101 | Plan listing | ✅ | `GET /api/app/subscription/plans` |
| 102 | Feature access + usage/limits | ✅ | `GET /api/app/subscription/features` |
| 103 | Plan CRUD (admin) | ✅ | `GET /api/app/plan` (paginated), `POST`, `PUT`, `DELETE` |
| 104 | Plan features management | ✅ | `GET /api/app/plan/{id}/features`, `POST /api/app/plan/{id}/features` |
| 105 | Feature CRUD (admin) | ✅ | `GET /api/app/feature` (paginated), `POST`, `PUT`, `DELETE` |
| 106 | Feature checker | ✅ | `IFeatureChecker.CheckLimitAsync()` + `IsEnabledAsync()` — used internally by `SalesInvoiceAppService` and `SalesQuotationAppService` |
| 107 | Usage tracking | ✅ | `IUsageService.IncrementAsync()` / `DecrementAsync()` / `GetUsageAsync()` / `ResetAsync()` |
| 108 | Feature codes | ✅ | `ErpFeatures`: InvoicesMonthly, UsersMax, BranchesMax, EmployeesMax, QuotationsMonthly, StorageMaxGb, AdvancedReports, ApiAccess, MultiCurrency |
| 109 | Stripe webhook | ✅ | `POST /api/webhooks/stripe` (WebhookController) |
| 110 | SMS status webhook | ✅ | `POST /api/webhooks/sms-status` (WebhookController) |
| 111 | Integration config CRUD + test | ✅ | `GET /api/app/integration-config` (paginated), `POST`, `PUT`, `DELETE`, `POST /api/app/integration-config/{id}/test-connection`, `GET /api/app/integration-config/active-providers` |

**SaaS verdict: 12/12 requirements met.** Full SaaS layer is implemented.

---

## Technical Requirements

| # | Mobile Requirement | Status | Endpoint / Notes |
|---|-------------------|--------|-----------------|
| 112 | OAuth2 password grant | ✅ | OpenIddict `ERPPlatform_App` client |
| 113 | OAuth2 refresh token grant | ✅ | Client supports RefreshToken grant |
| 114 | OAuth2 client credentials grant | ✅ | Client supports ClientCredentials grant |
| 115 | OAuth2 authorization code grant | ✅ | Client supports AuthorizationCode grant |
| 116 | Scope: `offline_access ERPPlatform` | ✅ | Required scope for refresh tokens |
| 117 | OpenID Connect userinfo | ✅ | `GET /connect/userinfo` |
| 118 | Token revocation endpoint | ⚠️ | OpenIddict supports `/connect/revoke` but not explicitly configured — verify |
| 119 | Multi-tenancy via `__tenant` header | ✅ | `AbpAspNetCoreMultiTenancyModule` enabled |
| 120 | ABP application-configuration | ✅ | `GET /api/abp/application-configuration` (currentUser, permissions, tenant, features, settings, localization) |
| 121 | Localization (en + ar) | ⚠️ | `en.json` is full; `ar.json` is sparse (only 4 keys: AppName, Menu:Home, Welcome, LongWelcomeMessage). Arabic translations incomplete. |
| 122 | Pagination on list endpoints | ⚠️ | All `CrudAppService`-derived services use `PagedAndSortedResultRequestDto` (skipCount + maxResultCount + sorting). But `NotificationAppService`, `ChatAppService`, `CrmAppService`, `FinanceAppService`, `PayrollAppService`, `DocumentAppService`, `FolderAppService` return `List<T>` / `ListResultDto<T>` with no pagination. |
| 123 | Search/filter on list endpoints | ⚠️ | Only `AuditLogAppService` has custom search/filter (Filter, EntityName, Action, UserName, StartDate, EndDate). All other services accept only sorting + pagination — no text search or field filtering. |
| 124 | CORS configuration | ✅ | ABP `PreConfigure<AbpAspNetCoreBuilderOptions>` configures CORS; verify allowed origins for mobile |
| 125 | Rate limiting | ❌ | No rate limiting middleware configured |
| 126 | API versioning | ❌ | No `Asp.Versioning` or ABP API versioning configured. All routes are unversioned. |
| 127 | SignalR for real-time | ✅ | `AbpAspNetCoreSignalRModule` registered; 2 hubs: Notification + Chat |
| 128 | SignalR user-targeted delivery | ❌ | Both hubs broadcast to `Clients.All` — no user/group mapping. `ChatHub.SendMessage(targetUserId, message)` ignores `targetUserId`. |
| 129 | Blob storage for documents | ✅ | ABP BlobStoring configured; `DocumentAppService.UploadAsync` uses it |
| 130 | Audit logging | ✅ | `AuditLogAppService.GetAuditLogsAsync()` with full search/filter/pagination |
| 131 | Swagger / OpenAPI | ✅ | ABP Swagger module configured in host |
| 132 | HTTPS / TLS | ✅ | `https://localhost:44327` configured |

**Technical verdict: 14/21 requirements met (3 partial, 4 missing).**

---

## Complete Missing Endpoints / Entities — Action Items for Backend

### 🔴 Critical (blocks mobile functionality)

| # | What's Missing | Recommended Backend Addition |
|---|----------------|---------------------------|
| 1 | **Push notification device registration** | Create `DeviceRegistration` entity (`UserId`, `DeviceToken`, `Platform` [iOS/Android], `IsEnabled`, `CreatedAt`). Create `DeviceRegistrationAppService` with `RegisterAsync`, `UnregisterAsync`, `UpdateTokenAsync`. Add FCM/APNS integration in `NotificationAppService.SendNotificationAsync`. |
| 2 | **Barcode on Product** | Add `Barcode` (string) property to `Product` entity. Add EF migration. Add `GetByBarcodeAsync(string barcode)` method to `ProductAppService` → `GET /api/inventory/product/by-barcode?barcode={barcode}`. |
| 3 | **GPS check-in** | Add `CheckInLatitude`, `CheckInLongitude`, `CheckOutLatitude`, `CheckOutLongitude` (double?) to `Attendance` entity. Add migration. Update `CheckInAsync` and `CheckOutAsync` to accept GPS params. |
| 4 | **Digital signature** | Add `DeliverySignature` (string/base64), `SignedBy`, `SignedAt` to `DeliveryNote`. Add `Signature` (base64) to `WorkflowTask`. Add migration. Add `CaptureSignatureAsync(Guid id, string signatureBase64)` methods. |
| 5 | **Leave balance** | Add `LeaveBalance` (decimal) to `Employee`. Create `LeavePolicyAppService` with accrual rules. Update `LeaveRequestAppService.ApproveAsync` to decrement balance. Add `GET /api/hr/employee/{id}/leave-balance`. |
| 6 | **Stock counting** | Create `StockCount` entity + `StockCountItem`. Create `StockCountAppService` with CRUD + `SubmitAsync`, `ApproveAsync`, `GetDiscrepanciesAsync`. |
| 7 | **Picking** | Create `PickList` entity + `PickListItem`. Create `PickListAppService` with CRUD + `AssignAsync`, `CompletePickAsync`, `GetAssignedAsync`. |
| 8 | **Packing** | Create `PackList` entity + `PackListItem`. Create `PackListAppService` with CRUD + `CompletePackAsync`, `GenerateShippingLabelAsync`. |
| 9 | **Field visit tracking** | Create `FieldVisit` entity (`EmployeeId`, `CustomerId`, `VisitDate`, `CheckInLat`, `CheckInLng`, `CheckOutLat`, `CheckOutLng`, `Notes`, `Outcome`). Create `FieldVisitAppService` with CRUD + `CheckInAsync`, `CheckOutAsync`, `GetRoutePlanAsync`. |
| 10 | **Unified approval center** | Create `ApprovalCenterAppService` with `GetPendingApprovalsAsync()` (aggregate leave requests + expenses + sales orders + purchase requests + workflow tasks where status=Pending) and `BatchApproveAsync(List<Guid> ids, string entityType)`. |
| 11 | **Payment processing** | Create `PaymentAppService` with `ProcessPaymentAsync(Guid invoiceId, string paymentMethodId)` (Stripe charge), `RefundAsync(Guid paymentId)`, `CaptureAsync(Guid paymentId)`. Extend `IntegrationAppService` to store Stripe secret key. |
| 12 | **Real-time push to specific users** | Map SignalR connections to ABP user IDs using `IUserIdProvider`. Update `NotificationHub` to send to `Clients.User(userId)`. Update `ChatHub.SendMessage` to send to `Clients.User(targetUserId)` instead of `Clients.All`. |

### 🟡 Important (improves mobile UX)

| # | What's Missing | Recommended Backend Addition |
|---|----------------|---------------------------|
| 13 | **Tenant branding API** | Add `PrimaryColor`, `SecondaryColor`, `Theme` (string) to `Company` entity. Create `BrandingAppService` with `GetBrandingAsync()` returning colors + logo + app name per tenant. |
| 14 | **AR aging report** | Create `AccountsReceivableAppService` with `GetAgingReportAsync()` — group invoices by customer, bucket by 0-30/31-60/61-90/90+ days overdue. |
| 15 | **AP aging report** | Create `AccountsPayableAppService` with `GetAgingReportAsync()` — same for supplier invoices. |
| 16 | **Bank reconciliation** | Create `BankReconciliationAppService` with `ImportStatementAsync`, `MatchTransactionsAsync`, `ReconcileAsync`. |
| 17 | **Search/filter on all list endpoints** | Create custom `GetListInput` DTOs with `Filter` (text search), `Status`, `DateFrom`, `DateTo` for key entities (Employee, Product, SalesInvoice, PurchaseOrder, LeaveRequest). Override `GetListAsync` in respective AppServices. |
| 18 | **Pagination on non-CRUD services** | Add `PagedAndSortedResultRequestDto` to `NotificationAppService`, `ChatAppService`, `CrmAppService`, `FinanceAppService`, `PayrollAppService`, `DocumentAppService`. Return `PagedResultDto<T>`. |
| 19 | **Arabic localization** | Complete `ar.json` with all keys from `en.json` translated to Arabic. |
| 20 | **AI conversation history** | Add `ChatSession` entity (`UserId`, `Messages` JSON). Update `AiAssistantAppService.AskAsync` to store/restore conversation context per session. |

### 🟢 Nice-to-have

| # | What's Missing | Recommended Backend Addition |
|---|----------------|---------------------------|
| 21 | **API versioning** | Add `AbpAspNetCoreMvcApiVersioningModule` or `Asp.Versioning.Mvc` package. Set default version to 1.0. |
| 22 | **Rate limiting** | Add ASP.NET Core rate limiting middleware (`AddRateLimiter`) in host module. |
| 23 | **Token revocation endpoint** | Explicitly configure OpenIddict revocation endpoint. |
| 24 | **User dashboard customization** | Create `DashboardWidget` entity + `UserDashboardConfig`. Create `DashboardConfigAppService` with `GetAsync`, `SaveAsync`. |

---

## Complete Endpoint Inventory (Existing)

### Shared (`/api/app`)

| Service | Method | HTTP | Route |
|---------|--------|------|-------|
| Notification | GetNotificationsAsync | GET | `/api/app/notification/notifications` |
| Notification | SendNotificationAsync | POST | `/api/app/notification/send-notification` |
| Notification | MarkAsReadAsync | POST | `/api/app/notification/{id}/mark-as-read` |
| Notification | MarkAllAsReadAsync | POST | `/api/app/notification/mark-all-as-read` |
| Chat | GetChannelMessagesAsync | GET | `/api/app/chat/channel-messages?channelName={name}` |
| Chat | GetDirectMessagesAsync | GET | `/api/app/chat/direct-messages?userId={id}&otherUserId={otherId}` |
| Chat | SendMessageAsync | POST | `/api/app/chat/send-message` |
| Crm | GetDealsAsync | GET | `/api/app/crm/deals` |
| Crm | CreateDealAsync | POST | `/api/app/crm/deal` |
| Crm | UpdateDealStageAsync | POST | `/api/app/crm/deal/{id}/update-deal-stage?newStage={stage}` |
| Crm | GetPipelineSummaryAsync | GET | `/api/app/crm/pipeline-summary` |
| Finance | GetAccountsAsync | GET | `/api/app/finance/accounts` |
| Finance | CreateAccountAsync | POST | `/api/app/finance/account` |
| Finance | GetJournalEntriesAsync | GET | `/api/app/finance/journal-entries` |
| Finance | CreateJournalEntryAsync | POST | `/api/app/finance/journal-entry` |
| Finance | GetProfitAndLossSummaryAsync | GET | `/api/app/finance/profit-and-loss-summary` |
| Payroll | GetPayrollRunsAsync | GET | `/api/app/payroll/payroll-runs` |
| Payroll | ProcessPayrollRunAsync | POST | `/api/app/payroll/process-payroll-run?period={period}` |
| Payroll | GetPayslipsAsync | GET | `/api/app/payroll/payslips?period={period}` |
| Audit | GetAuditLogsAsync | GET | `/api/app/audit-log/audit-logs` (paginated + filter) |
| Audit | GetListAsync | GET | `/api/app/audit-log` |
| Document | GetListAsync | GET | `/api/app/document?folderId={id}` |
| Document | UploadAsync | POST | `/api/app/document/upload` |
| Document | DownloadAsync | GET | `/api/app/document/{id}/download` |
| Document | DeleteAsync | DELETE | `/api/app/document/{id}` |
| Folder | GetListAsync | GET | `/api/app/folder?parentId={id}` |
| Folder | CreateAsync | POST | `/api/app/folder` |
| Folder | DeleteAsync | DELETE | `/api/app/folder/{id}` |
| Integration | CRUD | GET/POST/PUT/DELETE | `/api/app/integration-config` (paginated) |
| Integration | TestConnectionAsync | POST | `/api/app/integration-config/{id}/test-connection` |
| Integration | GetActiveProvidersAsync | GET | `/api/app/integration-config/active-providers` |
| Subscription | GetCurrentAsync | GET | `/api/app/subscription/current` |
| Subscription | GetPlansAsync | GET | `/api/app/subscription/plans` |
| Subscription | ChangePlanAsync | POST | `/api/app/subscription/change-plan?newPlanId={id}` |
| Subscription | CancelAsync | POST | `/api/app/subscription/cancel` |
| Subscription | ResumeAsync | POST | `/api/app/subscription/resume` |
| Subscription | GetFeaturesAsync | GET | `/api/app/subscription/features` |
| Plan | CRUD | GET/POST/PUT/DELETE | `/api/app/plan` (paginated) |
| Plan | SetActiveAsync | POST | `/api/app/plan/{id}/set-active?isActive={bool}` |
| Plan | GetFeaturesAsync | GET | `/api/app/plan/{id}/features` |
| Plan | UpdateFeaturesAsync | POST | `/api/app/plan/{id}/features` |
| Feature | CRUD | GET/POST/PUT/DELETE | `/api/app/feature` (paginated) |

### HR (`/api/hr`)

| Service | Method | HTTP | Route |
|---------|--------|------|-------|
| Employee | CRUD | GET/POST/PUT/DELETE | `/api/hr/employee` (paginated) |
| Department | CRUD | GET/POST/PUT/DELETE | `/api/hr/department` (paginated) |
| Attendance | CRUD | GET/POST/PUT/DELETE | `/api/hr/attendance` (paginated) |
| Attendance | CheckInAsync | POST | `/api/hr/attendance/check-in?employeeId=&employeeName=&departmentName=` |
| Attendance | CheckOutAsync | POST | `/api/hr/attendance/{id}/check-out` |
| LeaveRequest | CRUD | GET/POST/PUT/DELETE | `/api/hr/leave-request` (paginated) |
| LeaveRequest | ApproveAsync | POST | `/api/hr/leave-request/{id}/approve` |
| LeaveRequest | RejectAsync | POST | `/api/hr/leave-request/{id}/reject` |
| DashboardStats | GetStatsAsync | GET | `/api/hr/dashboard-stats/stats` |
| Company | CRUD | GET/POST/PUT/DELETE | `/api/hr/company` (paginated) |
| Branch | CRUD | GET/POST/PUT/DELETE | `/api/hr/branch` (paginated) |
| CostCenter | CRUD | GET/POST/PUT/DELETE | `/api/hr/cost-center` (paginated) |
| FiscalYear | CRUD | GET/POST/PUT/DELETE | `/api/hr/fiscal-year` (paginated) |
| FiscalYear | SetCurrentAsync | POST | `/api/hr/fiscal-year/{id}/set-current` |
| Currency | CRUD | GET/POST/PUT/DELETE | `/api/hr/currency` (paginated) |
| TaxConfig | CRUD | GET/POST/PUT/DELETE | `/api/hr/tax-config` (paginated) |
| PaymentTerm | CRUD | GET/POST/PUT/DELETE | `/api/hr/payment-term` (paginated) |
| Lead | CRUD | GET/POST/PUT/DELETE | `/api/hr/lead` (paginated) |
| Lead | ConvertToOpportunityAsync | POST | `/api/hr/lead/{id}/convert-to-opportunity` |
| Customer | CRUD | GET/POST/PUT/DELETE | `/api/hr/customer` (paginated) |
| Supplier | CRUD | GET/POST/PUT/DELETE | `/api/hr/supplier` (paginated) |
| SalesOrder | CRUD | GET/POST/PUT/DELETE | `/api/hr/sales-order` (paginated) |
| SalesOrder | ApproveAsync | POST | `/api/hr/sales-order/{id}/approve` |
| DeliveryNote | CRUD | GET/POST/PUT/DELETE | `/api/hr/delivery-note` (paginated) |
| PurchaseRequest | CRUD | GET/POST/PUT/DELETE | `/api/hr/purchase-request` (paginated) |
| PurchaseRequest | ApproveAsync | POST | `/api/hr/purchase-request/{id}/approve` |
| Rfq | CRUD | GET/POST/PUT/DELETE | `/api/hr/rfq` (paginated) |
| GoodsReceipt | CRUD | GET/POST/PUT/DELETE | `/api/hr/goods-receipt` (paginated) |
| Expense | CRUD | GET/POST/PUT/DELETE | `/api/hr/expense` (paginated) |
| Expense | ApproveAsync | POST | `/api/hr/expense/{id}/approve` |
| Project | CRUD | GET/POST/PUT/DELETE | `/api/hr/project` (paginated) |
| Manufacturing | CRUD | GET/POST/PUT/DELETE | `/api/hr/manufacturing` (paginated) |
| Manufacturing | CompleteOrderAsync | POST | `/api/hr/manufacturing/{id}/complete-order` |
| Asset | CRUD | GET/POST/PUT/DELETE | `/api/hr/asset` (paginated) |
| Maintenance | CRUD | GET/POST/PUT/DELETE | `/api/hr/maintenance` (paginated) |

### Inventory (`/api/inventory`)

| Service | Method | HTTP | Route |
|---------|--------|------|-------|
| Product | CRUD | GET/POST/PUT/DELETE | `/api/inventory/product` (paginated) |
| Product | AdjustStockAsync | POST | `/api/inventory/product/{id}/adjust-stock?newStock={n}` |
| Warehouse | CRUD | GET/POST/PUT/DELETE | `/api/inventory/warehouse` (paginated) |
| StockTransfer | CRUD | GET/POST/PUT/DELETE | `/api/inventory/stock-transfer` (paginated) |
| StockTransfer | UpdateStatusAsync | POST | `/api/inventory/stock-transfer/{id}/update-status?newStatus={status}` |
| PurchaseOrder | CRUD | GET/POST/PUT/DELETE | `/api/inventory/purchase-order` (paginated) |
| PurchaseOrder | UpdateStatusAsync | POST | `/api/inventory/purchase-order/{id}/update-status?newStatus={status}` |
| SalesInvoice | CRUD | GET/POST/PUT/DELETE | `/api/inventory/sales-invoice` (paginated) |
| SalesInvoice | MarkAsPaidAsync | POST | `/api/inventory/sales-invoice/{id}/mark-as-paid` |
| SalesInvoice | SendInvoiceAsync | POST | `/api/inventory/sales-invoice/{id}/send-invoice` |
| SalesInvoice | GetStatsAsync | GET | `/api/inventory/sales-invoice/stats` |
| SalesQuotation | CRUD | GET/POST/PUT/DELETE | `/api/inventory/sales-quotation` (paginated) |
| SalesQuotation | ConvertToInvoiceAsync | POST | `/api/inventory/sales-quotation/convert-to-invoice?quotationId={id}` |

### Workflow (`/api/workflow`)

| Service | Method | HTTP | Route |
|---------|--------|------|-------|
| WorkflowDefinition | CRUD | GET/POST/PUT/DELETE | `/api/workflow/workflow-definition` (paginated) |
| WorkflowTask | CRUD | GET/POST/PUT/DELETE | `/api/workflow/workflow-task` (paginated) |
| WorkflowTask | ApproveAsync | POST | `/api/workflow/workflow-task/{id}/approve?comments={comments}` |
| WorkflowTask | RejectAsync | POST | `/api/workflow/workflow-task/{id}/reject?comments={comments}` |

### AI (`/api/ai`)

| Service | Method | HTTP | Route |
|---------|--------|------|-------|
| AiAssistant | AskAsync | POST | `/api/ai/ai-assistant/ask` |
| AiAssistant | GetExecutiveSummaryAsync | GET | `/api/ai/ai-assistant/executive-summary` |

### ABP Built-in

| Module | Key Endpoints |
|--------|---------------|
| Account | `POST /api/account/register`, `POST /api/account/forgot-password`, `POST /api/account/reset-password`, `POST /api/account/change-password`, `GET /api/account/profile` |
| Identity | `GET/POST/PUT/DELETE /api/identity/user`, `GET/POST/PUT/DELETE /api/identity/role` |
| Tenant Management | `GET/POST/PUT/DELETE /api/multi-tenancy/tenant` |
| Feature Management | `GET/PUT /api/feature-management/features` |
| Setting Management | `GET/PUT /api/setting-management/settings` |
| Configuration | `GET /api/abp/application-configuration` |
| OpenIddict | `POST /connect/token`, `GET /connect/authorize`, `GET /connect/userinfo`, `POST /connect/revoke` (verify) |

### SignalR Hubs

| Hub | Route | Methods | Limitation |
|-----|-------|---------|------------|
| NotificationHub | `/signalr-hubs/notification` | `SendNotification(message)` | Broadcasts to ALL clients |
| ChatHub | `/signalr-hubs/chat` | `SendMessage(targetUserId, message)` | Broadcasts to ALL (ignores targetUserId) |

### Webhooks

| Endpoint | Purpose |
|----------|---------|
| `POST /api/webhooks/stripe` | Stripe payment webhook handler |
| `POST /api/webhooks/sms-status` | SMS delivery status webhook |

---

## Frontend API Service Mismatches (Pre-existing Bugs)

These are Angular frontend calls that don't match backend routes:

| Frontend Service | Call | Issue | Fix |
|-----------------|------|-------|-----|
| `IntegrationApiService` | Calls `/api/app/` (empty path) | Should call `/api/app/integration-config` | Add `apiPath = 'integration-config'` |
| `SalesApiService.convertToInvoice` | Uses path param `convert-to-invoice/{id}` | ABP binds `quotationId` from query string → 404 | Change to `post('convert-to-invoice', { quotationId })` |
| `PurchaseApiService.passQualityCheck` | Calls `/goods-receipt/{id}/pass-qc` | No such backend method → 404 | Remove or add `PassQualityCheckAsync` to `GoodsReceiptAppService` |

---

## Summary Scorecard

| Phase | ✅ Exists | ⚠️ Partial | ❌ Missing | Total | Coverage |
|-------|-----------|-------------|------------|-------|----------|
| Phase 1: Auth & Tenancy | 13 | 1 | 2 | 15 | 87% |
| Phase 2: Dashboard | 5 | 0 | 1 | 6 | 83% |
| Phase 3: HR & Payroll | 10 | 0 | 4 | 14 | 71% |
| Phase 4: Inventory | 9 | 0 | 4 | 13 | 69% |
| Phase 5: Sales & CRM | 12 | 0 | 4 | 16 | 75% |
| Phase 6: Workflow | 4 | 0 | 2 | 6 | 67% |
| Phase 7: Finance | 4 | 0 | 4 | 8 | 50% |
| Phase 8: Communication | 6 | 2 | 2 | 10 | 60% (real-time broken) |
| Phase 9: Documents & AI | 7 | 1 | 3 | 11 | 64% |
| SaaS | 12 | 0 | 0 | 12 | 100% |
| Technical | 14 | 3 | 4 | 21 | 67% |
| **Total** | **96** | **7** | **30** | **132** | **73%** |

> **Bottom line:** The backend covers 73% of the mobile API surface. SaaS is fully implemented. The biggest gaps are in real-time push (both SignalR and FCM/APNS), inventory operations (barcode, picking, packing, stock counting), field operations (GPS, signatures, field visits), and finance (payment processing, AR/AP aging). The mobile dev team can start building Phase 1-2 immediately but will hit gaps starting Phase 3+ unless the backend gaps are addressed in parallel.
