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
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Avatar { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Location { get; set; } = "Cairo HQ";
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
    public class PayrollRun : FullAuditedAggregateRoot<Guid>
    {
        public string Period { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public string Status { get; set; } = "Approved";
        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
    }

    public class Payslip : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Paid";
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
}
