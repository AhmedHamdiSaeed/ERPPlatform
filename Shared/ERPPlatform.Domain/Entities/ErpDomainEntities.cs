using System;
using Volo.Abp.Domain.Entities.Auditing;
using ERPPlatform.Domain.MultiCompany;

namespace ERPPlatform.Domain.Entities
{
    // SaaS Subscription & Features Domain Model
    public class Plan : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g. FREE, BASIC, PROFESSIONAL, ENTERPRISE
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string BillingPeriod { get; set; } = "Monthly"; // Monthly, Yearly, Custom
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }

    public class Feature : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g. ERP.Invoices.Monthly
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ValueType { get; set; } = "Integer"; // Boolean, Integer, Decimal, String, Unlimited
        public string Unit { get; set; } = "count"; // count, per_month, per_year, GB, boolean
        public bool IsActive { get; set; } = true;
    }

    public class PlanFeature : FullAuditedAggregateRoot<Guid>
    {
        public Guid PlanId { get; set; }
        public Guid FeatureId { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public long? LimitValue { get; set; } // Null for unlimited or boolean
        public string LimitType { get; set; } = "Monthly"; // Boolean, Fixed, Monthly, Yearly, Unlimited
    }

    public class Subscription : FullAuditedAggregateRoot<Guid>
    {
        public Guid? TenantId { get; set; }
        public Guid PlanId { get; set; }
        public string PlanCode { get; set; } = "FREE";
        public string Status { get; set; } = "Active"; // Trial, Active, PastDue, Suspended, Cancelled, Expired
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
        public DateTime CurrentPeriodEnd { get; set; } = DateTime.UtcNow.AddMonths(1);
        public bool AutoRenew { get; set; } = true;
        public string ExternalSubscriptionId { get; set; } = string.Empty;
        public DateTime? CancelledAt { get; set; }
    }

    public class SubscriptionHistory : FullAuditedAggregateRoot<Guid>
    {
        public Guid? TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid? OldPlanId { get; set; }
        public Guid NewPlanId { get; set; }
        public string Action { get; set; } = "Created"; // Created, Upgraded, Downgraded, Renewed, Cancelled, Suspended, Resumed, Expired
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; } = string.Empty;
    }

    public class UsageRecord : FullAuditedAggregateRoot<Guid>
    {
        public Guid? TenantId { get; set; }
        public Guid FeatureId { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long UsageValue { get; set; }
        public long? LimitValue { get; set; }
    }

    // HR Entities
    public class Employee : FullAuditedAggregateRoot<Guid>
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
        public decimal LeaveBalance { get; set; } = 21.0m; // Annual leave balance in days
    }

    public class Department : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal Budget { get; set; }
    }

    public class LeaveRequest : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = "Annual";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    public class Attendance : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CheckIn { get; set; } = string.Empty;
        public string CheckOut { get; set; } = string.Empty;
        public decimal WorkingHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Status { get; set; } = "Present";
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
    }

    // Organization Setup Entities
    public class Company : FullAuditedAggregateRoot<Guid>
    {
        public Guid? GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public string Currency { get; set; } = "USD";
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#1890ff";
        public string SecondaryColor { get; set; } = "#52c41a";
        public string Theme { get; set; } = "default"; // default, dark, compact
        public bool IsActive { get; set; } = true;
    }

    public class Branch : FullAuditedAggregateRoot<Guid>
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsHeadquarters { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class CostCenter : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class FiscalYear : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public bool IsClosed { get; set; } = false;
    }

    public class Currency : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = "$";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public bool IsBase { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class TaxConfig : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class PaymentTerm : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public int DueDays { get; set; } = 30;
        public int DiscountDays { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0.0m;
        public string Description { get; set; } = string.Empty;
    }

    // Inventory Entities
    public class Product : FullAuditedAggregateRoot<Guid>
    {
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string Unit { get; set; } = "pcs";
        public string WarehouseName { get; set; } = "Main Warehouse";
        public string Status { get; set; } = "In Stock";
        public string SupplierName { get; set; } = string.Empty;
    }

    public class Warehouse : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public int TotalProductsCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int CapacityPercentage { get; set; }
    }

    public class StockTransfer : FullAuditedAggregateRoot<Guid>
    {
        public string TransferCode { get; set; } = string.Empty;
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "In Transit";
    }

    public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
    {
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending Approval";
    }

    public class PurchaseRequest : FullAuditedAggregateRoot<Guid>
    {
        public string PrNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = "Pending Approval";
    }

    public class Rfq : FullAuditedAggregateRoot<Guid>
    {
        public string RfqNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DeadlineDate { get; set; } = DateTime.UtcNow.AddDays(10);
        public string Status { get; set; } = "Sent";
    }

    public class GoodsReceipt : FullAuditedAggregateRoot<Guid>
    {
        public string GrnNumber { get; set; } = string.Empty;
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        public string ReceivingWarehouse { get; set; } = "Main Warehouse";
        public string QcStatus { get; set; } = "Passed";
    }

    public class Supplier : FullAuditedAggregateRoot<Guid>
    {
        public string SupplierCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; } = 100000;
        public decimal OutstandingBalance { get; set; } = 0;
        public string PaymentTerms { get; set; } = "Net 30 Days";
        public bool IsActive { get; set; } = true;
    }

    // Sales & CRM Entities
    public class Deal : FullAuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Stage { get; set; } = "Prospecting";
        public int Probability { get; set; } = 20;
        public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public string OwnerName { get; set; } = string.Empty;

        // ── Opportunity enrichment (Phase 2) ───────────────────────────
        /// <summary>The account this opportunity belongs to, once the lead is converted.</summary>
        public Guid? CustomerId { get; set; }
        /// <summary>Primary contact for this opportunity.</summary>
        public Guid? ContactId { get; set; }
        /// <summary>The lead this opportunity was created from.</summary>
        public Guid? LeadId { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string Competitor { get; set; } = string.Empty;
        public string LostReason { get; set; } = string.Empty;
        public DateTime? ClosedAt { get; set; }
        /// <summary>Comma-separated tags, e.g. "enterprise,upsell".</summary>
        public string Tags { get; set; } = string.Empty;
    }

    public class Lead : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Source { get; set; } = "Website";
        public string Status { get; set; } = "New";
        public string SalespersonName { get; set; } = string.Empty;
        public DateTime NextFollowUp { get; set; } = DateTime.UtcNow.AddDays(3);
        public string Notes { get; set; } = string.Empty;

        // ── Lead qualification & conversion (Phase 1/2) ────────────────
        public Guid? OwnerUserId { get; set; }
        /// <summary>0-100 lead score; higher means hotter.</summary>
        public int Score { get; set; }
        public string LostReason { get; set; } = string.Empty;
        public DateTime? QualifiedAt { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public Guid? ConvertedCustomerId { get; set; }
        public Guid? ConvertedDealId { get; set; }
        /// <summary>Comma-separated tags, e.g. "hot,referral".</summary>
        public string Tags { get; set; } = string.Empty;
    }

    public class Customer : FullAuditedAggregateRoot<Guid>
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; } = 50000;
        public decimal OutstandingBalance { get; set; } = 0;
        public string PaymentTerms { get; set; } = "Net 30 Days";
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; } = true;

        // ── Account enrichment (Phase 1) ───────────────────────────────
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public Guid? OwnerUserId { get; set; }
        public bool IsVip { get; set; }
        /// <summary>Comma-separated tags, e.g. "vip,manufacturing".</summary>
        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>
    /// A person attached to a lead or an account. "Contacts" in the CRM roadmap.
    /// </summary>
    public class CrmContact : FullAuditedAggregateRoot<Guid>
    {
        public Guid? CustomerId { get; set; }
        public Guid? LeadId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        /// <summary>Marks the default contact shown on the customer card.</summary>
        public bool IsPrimary { get; set; }
        public string Notes { get; set; } = string.Empty;

        /// <summary>Denormalised "First Last" kept in sync by CrmContactAppService.</summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// A call, meeting, task, email or follow-up. Activities are the backbone of the
    /// customer timeline and of the "overdue follow-up" KPI.
    /// </summary>
    public class CrmActivity : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>Call | Meeting | Task | Email | FollowUp</summary>
        public string Type { get; set; } = "Task";
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(1);
        /// <summary>Open | Completed | Cancelled</summary>
        public string Status { get; set; } = "Open";
        public DateTime? CompletedAt { get; set; }
        /// <summary>Low | Normal | High</summary>
        public string Priority { get; set; } = "Normal";

        // Polymorphic link — normally exactly one of these is set.
        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }

        public string AssignedTo { get; set; } = string.Empty;
        public Guid? AssignedToUserId { get; set; }
        public string Outcome { get; set; } = string.Empty;
    }

    /// <summary>
    /// Free-text note (optionally with one attachment) attached to any CRM record.
    /// </summary>
    public class CrmNote : FullAuditedAggregateRoot<Guid>
    {
        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public string AttachmentName { get; set; } = string.Empty;
        public string AttachmentUrl { get; set; } = string.Empty;
    }

    public class SalesOrder : FullAuditedAggregateRoot<Guid>
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpectedDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
    }

    public class DeliveryNote : FullAuditedAggregateRoot<Guid>
    {
        public string DeliveryNumber { get; set; } = string.Empty;
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
        public string DispatchWarehouse { get; set; } = "Main Warehouse";
        public string Status { get; set; } = "Dispatched";
        public string Carrier { get; set; } = "Express Freight";
        public string TrackingNumber { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string DeliveryProof { get; set; } = string.Empty; // base64 image or document URL
        public string SignatureBase64 { get; set; } = string.Empty; // captured digital signature
        public string SignedBy { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
    }

    public class SalesInvoice : FullAuditedAggregateRoot<Guid>
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class SalesQuotation : FullAuditedAggregateRoot<Guid>
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    // Workflow Entities
    public class WorkflowDefinition : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Status { get; set; } = "Active";
        public string GraphJson { get; set; } = "{}";
        public int Version { get; set; } = 1;
    }

    public class WorkflowTask : FullAuditedAggregateRoot<Guid>
    {
        public string TaskNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByAvatar { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public string Comments { get; set; } = string.Empty;
        public string SignatureBase64 { get; set; } = string.Empty;
        public string SignedBy { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
    }

    // Finance & Accounting Entities
    public class Account : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Asset";
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";
        public string ParentCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class JournalEntry : FullAuditedAggregateRoot<Guid>
    {
        public string EntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string Status { get; set; } = "Posted";
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ExpenseRequest : FullAuditedAggregateRoot<Guid>
    {
        public string ExpenseCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Category { get; set; } = "Travel";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending Approval";
    }

    // Projects & Manufacturing Entities
    public class Project : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal SpentAmount { get; set; }
        public int ProgressPercentage { get; set; } = 0;
        public string Status { get; set; } = "In Progress";
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddMonths(3);
    }

    public class BillOfMaterials : FullAuditedAggregateRoot<Guid>
    {
        public string BomCode { get; set; } = string.Empty;
        public string FinishedProductName { get; set; } = string.Empty;
        public string RawMaterialName { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; } = 1;
        public decimal UnitCost { get; set; }
    }

    public class ManufacturingOrder : FullAuditedAggregateRoot<Guid>
    {
        public string MoNumber { get; set; } = string.Empty;
        public string FinishedProductName { get; set; } = string.Empty;
        public int QuantityToProduce { get; set; } = 100;
        public string WorkCenter { get; set; } = "Assembly Line A";
        public DateTime ScheduledStartDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "In Production";
    }

    // Fixed Assets & Maintenance Entities
    public class FixedAsset : FullAuditedAggregateRoot<Guid>
    {
        public string AssetCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Machinery";
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public decimal PurchaseCost { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal DepreciationRateAnnual { get; set; } = 15.0m;
        public string Location { get; set; } = "Cairo Plant";
        public string AssignedEmployee { get; set; } = string.Empty;
    }

    public class MaintenanceRequest : FullAuditedAggregateRoot<Guid>
    {
        public string WorkOrderCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = "Preventive";
        public string TechnicianName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Status { get; set; } = "In Progress";
        public DateTime ScheduledDate { get; set; } = DateTime.UtcNow;
    }

    // Payroll Entities
    /// <summary>
    /// One payroll run for one period (e.g. "2026-09").
    /// Status: Draft → Calculated → HrReview → FinanceReview → Finalized → Posted.
    /// </summary>
    public class PayrollRun : FullAuditedAggregateRoot<Guid>
    {
        public string Period { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        /// <summary>Gross + employer contributions — what the period actually costs the company.</summary>
        public decimal TotalEmployerCost { get; set; }
        public string Status { get; set; } = "Approved";
        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
        /// <summary>Set when the run is finalized; finalized runs can only be corrected by reversal.</summary>
        public DateTime? LockedAt { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class Payslip : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal EmployerCost { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = "Paid";
        public Guid? PayrollRunId { get; set; }
    }

    // Enhanced Audit Log Entity (Module 25)
    public class AuditLogEntry : FullAuditedAggregateRoot<Guid>
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = "Updated"; // Created, Updated, Deleted
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ChangesJson { get; set; } = "{}";
        public string OldValues { get; set; } = "{}";
        public string NewValues { get; set; } = "{}";
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

    // Enterprise Multi-Company Group Entity (Module 26)
    public class CompanyGroup : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    // User Branch Access Matrix (Module 26)
    public class UserBranchAssignment : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    // External API Integration Credentials & Settings (Module 27)
    public class IntegrationConfig : FullAuditedAggregateRoot<Guid>
    {
        public Guid? TenantId { get; set; }
        public string ProviderType { get; set; } = "PaymentGateway"; // PaymentGateway, SMS, WhatsApp, Email, TaxAuthority
        public string ProviderName { get; set; } = "Stripe"; // Stripe, PayPal, Twilio, SendGrid, Infobip
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ConfigJson { get; set; } = "{}";
    }

    // Real-Time Chat & Notification Entities
    public enum ChatConversationType
    {
        Direct = 0,
        Group = 1,
        Channel = 2
    }

    // A conversation is either a direct (1:1) thread, a group (multiple named members)
    // or an open channel. Direct conversations reuse the Name field to store a
    // deterministic "dm:<sortedUserIdA>:<sortedUserIdB>" key so a 1:1 thread is never duplicated.
    public class ChatConversation : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public ChatConversationType Type { get; set; } = ChatConversationType.Group;
        public string CreatorUserId { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public string LastMessageSenderName { get; set; } = string.Empty;
        public string LastMessagePreview { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
    }

    // Membership of a user in a conversation. Also carries per-user read state
    // (LastReadAt) which drives unread badges, and mute state for notifications.
    public class ChatParticipant : FullAuditedAggregateRoot<Guid>
    {
        public Guid ConversationId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastReadAt { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? LeftAt { get; set; }
    }

    public class ChatMessage : FullAuditedAggregateRoot<Guid>
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // --- Group chat extensions ---
        public Guid? ConversationId { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }

        // NOTE: deliberately NOT named "IsDeleted" - that name is taken by ABP's
        // ISoftDelete on FullAuditedAggregateRoot, and setting it would hide the
        // row from every query via the soft-delete data filter. Soft-deleting a
        // chat message would remove it from history instead of showing a
        // "message deleted" placeholder, so we track the flag separately.
        public bool IsDeletedBySender { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Comma separated list of mentioned user ids, e.g. "<id1>,<id2>"
        public string MentionedUserIds { get; set; } = string.Empty;

        // Optional single attachment (BLOB stored)
        public string AttachmentName { get; set; } = string.Empty;
        public string AttachmentBlobName { get; set; } = string.Empty;
        public string AttachmentContentType { get; set; } = string.Empty;
        public long AttachmentSizeBytes { get; set; }
    }

    public class ChatMessageReaction : FullAuditedAggregateRoot<Guid>
    {
        public Guid MessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
    }

    public class SystemNotification : FullAuditedAggregateRoot<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "System";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }

    // Push Notification Device Registration Entity
    public class DeviceRegistration : FullAuditedAggregateRoot<Guid>
    {
        public Guid? UserId { get; set; }
        public string DeviceToken { get; set; } = string.Empty; // FCM or APNS token
        public string Platform { get; set; } = "Android"; // Android, iOS
        public string DeviceName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public DateTime? LastRegisteredAt { get; set; } = DateTime.UtcNow;
    }

    // Stock Count / Cycle Count Entities
    public class StockCount : FullAuditedAggregateRoot<Guid>
    {
        public string CountNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime CountDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Draft"; // Draft, InProgress, Submitted, Approved, Rejected
        public string CountedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class StockCountItem : FullAuditedAggregateRoot<Guid>
    {
        public Guid StockCountId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int SystemStock { get; set; }
        public int CountedStock { get; set; }
        public int Discrepancy { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // Picking / Pick List Entities
    public class PickList : FullAuditedAggregateRoot<Guid>
    {
        public string PickNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Open"; // Open, Assigned, InProgress, Completed, Cancelled
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class PickListItem : FullAuditedAggregateRoot<Guid>
    {
        public Guid PickListId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int PickedQuantity { get; set; }
        public string BinLocation { get; set; } = string.Empty;
        public bool IsPicked { get; set; } = false;
    }

    // Packing / Pack List Entities
    public class PackList : FullAuditedAggregateRoot<Guid>
    {
        public string PackNumber { get; set; } = string.Empty;
        public Guid? PickListId { get; set; }
        public string PickListNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PackedBy { get; set; } = string.Empty;
        public DateTime PackedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Open"; // Open, Packed, Shipped, Cancelled
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string ShippingLabelUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PackListItem : FullAuditedAggregateRoot<Guid>
    {
        public Guid PackListId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int PackedQuantity { get; set; }
        public int ShippedQuantity { get; set; }
        public string PackageType { get; set; } = "Carton"; // Carton, Pallet, Box, Envelope
        public decimal WeightKg { get; set; }
    }

    // Field Visit / Customer Visit Tracking Entity
    public class FieldVisit : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; } = DateTime.UtcNow;
        public string Purpose { get; set; } = string.Empty; // Sales, Support, Delivery, Audit
        public DateTime? CheckInTime { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty; // Positive, Neutral, Negative, Follow-up Needed
        public string Status { get; set; } = "Planned"; // Planned, CheckedIn, Completed, Cancelled
        public string NextFollowUpDate { get; set; } = string.Empty;
    }

    // Payment Entity
    public class Payment : FullAuditedAggregateRoot<Guid>
    {
        public string PaymentNumber { get; set; } = string.Empty;
        public Guid? SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = "Card"; // Card, BankTransfer, Cash, Cheque
        public string Provider { get; set; } = "Stripe"; // Stripe, PayPal, Manual
        public string ExternalPaymentId { get; set; } = string.Empty; // Stripe charge/payment intent ID
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded, PartiallyRefunded
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // AI Chat Session Entity
    public class AiChatSession : FullAuditedAggregateRoot<Guid>
    {
        public Guid? UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
        public string MessagesJson { get; set; } = "[]"; // JSON array of {role, content, timestamp}
        public string Status { get; set; } = "Active"; // Active, Archived
    }

    // Dashboard Widget Catalog Entity
    public class DashboardWidget : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty; // e.g. "sales-summary", "hr-stats", "pending-approvals"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // Sales, HR, Inventory, Finance, Workflow, AI
        public string ComponentName { get; set; } = string.Empty; // Angular component name to render
        public string Icon { get; set; } = string.Empty; // e.g. "fas fa-chart-line"
        public int DefaultWidth { get; set; } = 6; // 1-12 grid columns
        public int DefaultHeight { get; set; } = 300; // px
        public int DefaultOrder { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
    }

    // User Dashboard Configuration Entity (per-user widget layout)
    public class UserDashboardConfig : FullAuditedAggregateRoot<Guid>
    {
        public Guid? UserId { get; set; }
        public string DashboardName { get; set; } = "Default"; // Default, Executive, Sales, HR, etc.
        public string LayoutJson { get; set; } = "[]"; // JSON array of {widgetCode, x, y, w, h, order}
        public bool IsDefault { get; set; } = false;
    }

    // Recruitment / Applicant Tracking Entity
    public class Candidate : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied"; // Applied, Screening, Interview, Technical, Offer, Hired
        public decimal Rating { get; set; }
        public string SkillsJson { get; set; } = "[]"; // JSON string array of skill names
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;
    }

    // Workflow Execution History Entities
    public class WorkflowExecutionLog : FullAuditedAggregateRoot<Guid>
    {
        public string ExecutionCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public string TriggeredBy { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public string Duration { get; set; } = string.Empty; // Display string, e.g. "4m 12s"
        public string Status { get; set; } = "Running"; // Running, Completed, Failed
    }

    public class WorkflowExecutionStep : FullAuditedAggregateRoot<Guid>
    {
        public Guid WorkflowExecutionLogId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Passed"; // Passed, Failed, Running, Skipped
        public string Details { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    // Report Catalog Entity
    public class ReportDefinition : FullAuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // HR, Inventory, Workflow, Financial
        public string Description { get; set; } = string.Empty;
        public DateTime? LastGenerated { get; set; }
        public int RecordCount { get; set; }
        public string DataSourceCode { get; set; } = string.Empty; // Key the report runner resolves
        public bool IsEnabled { get; set; } = true;
    }

    // Leave Policy Entity (accrual rules per leave type)
    public class LeavePolicy : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty; // e.g. "Annual Leave Policy", "Sick Leave Policy"
        public string LeaveType { get; set; } = "Annual"; // Annual, Sick, Casual, Maternity, Paternity, Unpaid
        public decimal AnnualAccrualDays { get; set; } = 21.0m; // Days accrued per year
        public decimal MaxCarryForwardDays { get; set; } = 5.0m; // Max days that can be carried to next year
        public decimal MaxConsecutiveDays { get; set; } = 30.0m; // Max consecutive leave days allowed
        public bool RequiresApproval { get; set; } = true;
        public bool AllowHalfDay { get; set; } = true;
        public int ProbationPeriodMonths { get; set; } = 3; // Months before leave is available
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }

    // ────────────────────────────────────────────────────────────────
    // Payroll — salary components, employee overrides, payslip detail,
    // and the pre-run anomaly list.
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// A configurable earning, deduction or employer contribution. Nothing about pay
    /// is hard-coded: the payroll engine reads the active components for the period.
    /// </summary>
    public class SalaryComponent : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>Stable code used by formulas and reports, e.g. "BASIC", "HOUSING", "SI".</summary>
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        /// <summary>Earning | Deduction | EmployerContribution</summary>
        public string Type { get; set; } = "Earning";
        /// <summary>Fixed | Percentage | Formula (Formula is reserved for the rule builder)</summary>
        public string CalculationType { get; set; } = "Fixed";
        /// <summary>Fixed amount, or the percentage value (20 means 20%).</summary>
        public decimal Amount { get; set; }
        /// <summary>For Percentage components: the code of the component it is calculated from.</summary>
        public string PercentageOfComponentCode { get; set; } = "BASIC";
        public string Formula { get; set; } = string.Empty;
        public bool IsTaxable { get; set; } = true;
        public bool IsRecurring { get; set; } = true;
        public bool CountsAsEmployerCost { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; } = new DateTime(2000, 1, 1);
        public DateTime? EffectiveTo { get; set; }
        public string GlAccount { get; set; } = string.Empty;
        public string CostCenter { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Employee-specific component value: an override of the default amount, or an
    /// extra component (one-off bonus, loan installment) applied to one person.
    /// </summary>
    public class EmployeeSalaryComponent : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid ComponentId { get; set; }
        public string ComponentCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; } = new DateTime(2000, 1, 1);
        public DateTime? EffectiveTo { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>One row on a payslip, so the numbers are explainable rather than opaque.</summary>
    public class PayslipLine : FullAuditedAggregateRoot<Guid>
    {
        public Guid PayslipId { get; set; }
        public string ComponentCode { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;
        /// <summary>Earning | Deduction | EmployerContribution</summary>
        public string Type { get; set; } = "Earning";
        public decimal Amount { get; set; }
        public bool IsTaxable { get; set; }
    }

    /// <summary>
    /// Something worth a human look before payroll is finalized. Produced by the
    /// preview/calculation pass; HR resolves or dismisses each one.
    /// </summary>
    public class PayrollAnomaly : FullAuditedAggregateRoot<Guid>
    {
        public string Period { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        /// <summary>SalarySpike | HighOvertime | NegativeNet | MissingBankAccount | DuplicateEmployee | TerminatedButPaid | MissingAttendance | UnexpectedDeduction | HighBonus</summary>
        public string Kind { get; set; } = string.Empty;
        /// <summary>Warning | Critical</summary>
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
        public decimal Delta { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionNote { get; set; } = string.Empty;
    }
}
