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

    public class TenantProfile : FullAuditedAggregateRoot<Guid>
    {
        public Guid TenantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string CustomDomain { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string Timezone { get; set; } = "UTC";
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#4f46e5";
        public string PlanTier { get; set; } = "Starter"; // Trial, Starter, Professional, Enterprise
        public string Status { get; set; } = "Active"; // Active, Suspended, Trial, Expired
        public int MaxUsers { get; set; } = 10;
        public int StorageLimitGb { get; set; } = 10;
        public double UsedStorageMb { get; set; } = 120.5;
        public int ActiveUserCount { get; set; } = 1;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminPhone { get; set; } = string.Empty;
        public bool IsDedicatedDb { get; set; } = false;
        public string CustomConnectionString { get; set; } = string.Empty;
        public string EnabledModulesJson { get; set; } = "[\"HR\",\"Finance\",\"Sales\",\"Inventory\",\"AI\",\"Workflow\"]";
        public DateTime? TrialEndDate { get; set; }
        public DateTime? SubscriptionRenewalDate { get; set; }
        public decimal MonthlyFee { get; set; } = 99m;
    }

    // HR Entities & Core Master Data (Phase 1 Enterprise Suite)
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
        public string Status { get; set; } = "Active"; // Active, On Leave, Inactive, Suspended, Terminated
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
        public decimal LeaveBalance { get; set; } = 21.0m; // Annual leave balance in days

        // Master Data Enrichment
        public string NationalId { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = "Egyptian";
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Male"; // Male, Female
        public string MaritalStatus { get; set; } = "Single"; // Single, Married, Divorced, Widowed
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmergencyContactRelation { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = "FullTime"; // FullTime, PartTime, Contractor, Intern, Remote
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string CostCenterCode { get; set; } = string.Empty;
        public int NoticePeriodDays { get; set; } = 30;
    }

    public class JobGrade : FullAuditedAggregateRoot<Guid>
    {
        public string GradeCode { get; set; } = string.Empty; // e.g. "GR-1", "L3"
        public string GradeName { get; set; } = string.Empty; // e.g. "Senior Associate", "Lead"
        public string Level { get; set; } = "Mid"; // Entry, Mid, Senior, Executive
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class JobPosition : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? JobGradeId { get; set; }
        public string JobGradeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmployeeContract : FullAuditedAggregateRoot<Guid>
    {
        public string ContractNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ContractType { get; set; } = "Permanent"; // Permanent, FixedTerm, Probation, Contractor
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportationAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TotalGrossSalary => BasicSalary + HousingAllowance + TransportationAllowance + OtherAllowances;
        public int WorkingHoursPerWeek { get; set; } = 40;
        public int NoticePeriodDays { get; set; } = 30;
        public string Status { get; set; } = "Active"; // Active, Expired, Terminated, Draft, Renewed
        public DateTime? SignedAt { get; set; }
        public string SignedDocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class HrAction : FullAuditedAggregateRoot<Guid>
    {
        public string ActionCode { get; set; } = string.Empty; // e.g. "HRA-2026-001"
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Promotion"; // Hire, Promotion, Transfer, SalaryChange, DepartmentChange, ManagerChange, Suspension, Termination, Resignation
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Approved"; // Pending, Approved, Rejected, Implemented
        public string PreviousValuesJson { get; set; } = "{}"; // JSON snapshot of previous values
        public string NewValuesJson { get; set; } = "{}"; // JSON snapshot of new values
        public string RequestedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class EmployeeDocument : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "NationalId"; // NationalId, Passport, Contract, Medical, Certificate, Degree, Tax, Visa, Warning, Other
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsVerified { get; set; } = false;
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string Notes { get; set; } = string.Empty;
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
        public bool IsActiveVersion { get; set; } = true;
        public Guid? ParentDefinitionId { get; set; }
        public string VersionNotes { get; set; } = string.Empty;
        public string TriggerType { get; set; } = "Manual"; // Manual, EntityCreated, EntityUpdated, Webhook, Schedule, Cron, EntityCdc
        public string ExecutionMode { get; set; } = "Sequential"; // Sequential, Parallel, EventDriven
        public int SlaHours { get; set; } = 24;
        public string WebhookSecret { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string CdcEntityName { get; set; } = string.Empty;
        public string CdcEvent { get; set; } = string.Empty;
    }

    public class WorkflowTask : FullAuditedAggregateRoot<Guid>
    {
        public string TaskNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByAvatar { get; set; } = string.Empty;
        public string AssignedToRole { get; set; } = string.Empty;
        public string AssignedToUserId { get; set; } = string.Empty;
        public string OriginalAssigneeId { get; set; } = string.Empty;
        public string DelegatedFromUserId { get; set; } = string.Empty;
        public bool IsEscalated { get; set; } = false;
        public string EscalatedToUserId { get; set; } = string.Empty;
        public DateTime? EscalatedAt { get; set; }
        public string ActionToken { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public int StepOrder { get; set; } = 1;
        public string CurrentNodeId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, ChangesRequested
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

        // --- Enterprise ERP Chat extensions ---
        public bool IsPinned { get; set; }
        public string CardDataJson { get; set; } = string.Empty;
        public bool IsAiResponse { get; set; }
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

    // Recruitment & ATS Domain Entities
    public class JobRequisition : FullAuditedAggregateRoot<Guid>
    {
        public string RequisitionCode { get; set; } = string.Empty; // e.g. "REQ-2026-001"
        public string Title { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int VacanciesCount { get; set; } = 1;
        public string EmploymentType { get; set; } = "FullTime";
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string ExperienceLevel { get; set; } = "Mid";
        public string JobDescription { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string HiringManager { get; set; } = string.Empty;
        public string Status { get; set; } = "Open"; // Draft, Open, InProgress, Filled, Cancelled
        public DateTime TargetStartDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    }

    public class Candidate : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AppliedPosition { get; set; } = string.Empty;
        public Guid? JobRequisitionId { get; set; }
        public decimal ExperienceYears { get; set; }
        public string Stage { get; set; } = "Applied"; // Applied, Screening, Interview, Technical, Offer, Hired, Rejected
        public decimal Rating { get; set; }
        public string SkillsJson { get; set; } = "[]"; // JSON string array of skill names
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public string CvUrl { get; set; } = string.Empty;
        public string ExpectedSalary { get; set; } = string.Empty;
        public string NoticePeriod { get; set; } = "1 Month";
        public string Notes { get; set; } = string.Empty;
        public Guid? ConvertedEmployeeId { get; set; }
    }

    public class Interview : FullAuditedAggregateRoot<Guid>
    {
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string InterviewType { get; set; } = "Technical"; // Screening, Technical, HR, Managerial, Final
        public DateTime ScheduledTime { get; set; } = DateTime.UtcNow.AddDays(2);
        public string InterviewerName { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;
        public decimal Score { get; set; } // 1 - 10
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Rescheduled
        public string Recommendation { get; set; } = "Pending"; // Advance, StrongHire, Hire, Hold, Reject
        public string FeedbackNotes { get; set; } = string.Empty;
    }

    public class OfferLetter : FullAuditedAggregateRoot<Guid>
    {
        public string OfferCode { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal OfferedBasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal TotalMonthlyPackage => OfferedBasicSalary + HousingAllowance + TransportAllowance;
        public DateTime ProposedStartDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Declined, Expired
        public DateTime? AcceptedAt { get; set; }
        public string SignedDocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // Onboarding & Offboarding Lifecycle Entities
    public class OnboardingTask : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "IT"; // IT, HR, Finance, Admin, Department
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Blocked
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class OffboardingRequest : FullAuditedAggregateRoot<Guid>
    {
        public string RequestNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime ResignationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastWorkingDay { get; set; } = DateTime.UtcNow.AddDays(30);
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "ClearanceInProgress"; // Submitted, ClearanceInProgress, FinanceApproved, HandoverComplete, Finalized, Cancelled
        public string ItClearanceStatus { get; set; } = "Pending"; // Pending, Cleared
        public string AdminClearanceStatus { get; set; } = "Pending";
        public string FinanceClearanceStatus { get; set; } = "Pending";
        public decimal OutstandingLoanBalance { get; set; }
        public decimal AccruedLeavePayout { get; set; }
        public decimal EndOfServiceGratuity { get; set; }
        public decimal NetFinalSettlement => AccruedLeavePayout + EndOfServiceGratuity - OutstandingLoanBalance;
        public string ExitInterviewNotes { get; set; } = string.Empty;
    }

    // Employee Financial Requests & Loans
    public class EmployeeLoan : FullAuditedAggregateRoot<Guid>
    {
        public string LoanNumber { get; set; } = string.Empty; // e.g. "LN-2026-001"
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LoanType { get; set; } = "Personal"; // Personal, Advance, Emergency, Housing
        public decimal PrincipalAmount { get; set; }
        public int TotalInstallments { get; set; } = 12;
        public decimal MonthlyInstallment => TotalInstallments > 0 ? (PrincipalAmount / TotalInstallments) : 0;
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingBalance => PrincipalAmount - TotalPaidAmount;
        public DateTime StartDeductionPeriod { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "PendingApproval"; // PendingApproval, Approved, Active, FullyRepaid, Rejected
        public string ApprovedBy { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }

    public class LoanInstallment : FullAuditedAggregateRoot<Guid>
    {
        public Guid LoanId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Period { get; set; } = string.Empty; // e.g. "2026-09"
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsDeducted { get; set; } = false;
        public Guid? PayrollRunId { get; set; }
        public DateTime? DeductedAt { get; set; }
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

    // ────────────────────────────────────────────────────────────────
    // Performance Management & Goals / KPIs (Module 11)
    // ────────────────────────────────────────────────────────────────
    public class PerformanceReview : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ReviewCycle { get; set; } = "2026 Annual"; // e.g. "2026 Q1", "2026 Annual"
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow.AddMonths(-6);
        public DateTime PeriodEnd { get; set; } = DateTime.UtcNow;
        public string ReviewerName { get; set; } = string.Empty;
        public decimal SelfRating { get; set; } // 1.0 - 5.0
        public decimal ManagerRating { get; set; } // 1.0 - 5.0
        public decimal FinalRating { get; set; } // 1.0 - 5.0
        public string Status { get; set; } = "Draft"; // Draft, SelfAssessment, ManagerReview, Completed
        public decimal GoalsAchievedPercentage { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string AreasForImprovement { get; set; } = string.Empty;
        public bool PromotionRecommended { get; set; } = false;
        public string ManagerFeedback { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }

    public class PerformanceGoal : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Operational"; // Strategic, Operational, Learning, KPI
        public int Weight { get; set; } = 20; // Percentage weight (e.g. 20%)
        public decimal TargetValue { get; set; } = 100;
        public decimal CurrentValue { get; set; } = 0;
        public string MetricUnit { get; set; } = "%";
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddMonths(3);
        public string Status { get; set; } = "InProgress"; // NotStarted, InProgress, Achieved, Behind
        public decimal Score { get; set; } // 1 - 5
    }

    // ────────────────────────────────────────────────────────────────
    // Learning & Development / Training (Module 12)
    // ────────────────────────────────────────────────────────────────
    public class TrainingCourse : FullAuditedAggregateRoot<Guid>
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Technical"; // Technical, Compliance, Leadership, SoftSkills
        public string TrainerName { get; set; } = string.Empty;
        public int DurationHours { get; set; } = 16;
        public decimal CostPerAttendee { get; set; } = 0;
        public int MaxAttendees { get; set; } = 25;
        public string DeliveryMethod { get; set; } = "Online"; // Online, Classroom, Hybrid
        public string Status { get; set; } = "Active"; // Active, Upcoming, Completed, Archived
        public decimal PassingScore { get; set; } = 70.0m;
    }

    public class TrainingEnrollment : FullAuditedAggregateRoot<Guid>
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = "Enrolled"; // Enrolled, InProgress, Completed, Failed, Cancelled
        public decimal Score { get; set; }
        public bool CertificateIssued { get; set; } = false;
        public string Feedback { get; set; } = string.Empty;
    }

    public class EmployeeCertification : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string CertificationName { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public string CredentialId { get; set; } = string.Empty;
        public string CertificateUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, ExpiringSoon, Expired
    }

    // ────────────────────────────────────────────────────────────────
    // Benefits & Corporate Insurance (Module 13)
    // ────────────────────────────────────────────────────────────────
    public class BenefitPlan : FullAuditedAggregateRoot<Guid>
    {
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Category { get; set; } = "MedicalInsurance"; // MedicalInsurance, LifeInsurance, Retirement, GymWellness, Allowance
        public string ProviderName { get; set; } = string.Empty;
        public string CoverageDetails { get; set; } = string.Empty;
        public decimal EmployerContributionMonthly { get; set; }
        public decimal EmployeeContributionMonthly { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmployeeBenefit : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid BenefitPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string Category { get; set; } = "MedicalInsurance";
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public decimal CoverageAmount { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeDeduction { get; set; }
        public string Status { get; set; } = "Active"; // Active, Terminated, Suspended
    }

    // ────────────────────────────────────────────────────────────────
    // Work Shifts & Scheduling (Module 6)
    // ────────────────────────────────────────────────────────────────
    public class WorkShift : FullAuditedAggregateRoot<Guid>
    {
        public string ShiftCode { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int BreakMinutes { get; set; } = 60;
        public bool IsNightShift { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class ShiftAssignment : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid WorkShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
