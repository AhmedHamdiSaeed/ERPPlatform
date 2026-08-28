using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Documents;

namespace ERPPlatform.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class ERPPlatformDbContext :
    AbpDbContext<ERPPlatformDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    // SaaS Subscription & Features Module DbSets
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<PlanFeature> PlanFeatures { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }
    public DbSet<UsageRecord> UsageRecords { get; set; }

    // HR Module DbSets
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<Attendance> Attendances { get; set; }

    // Organization Setup DbSets
    public DbSet<CompanyGroup> CompanyGroups { get; set; }
    public DbSet<UserBranchAssignment> UserBranchAssignments { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<TaxConfig> TaxConfigs { get; set; }
    public DbSet<PaymentTerm> PaymentTerms { get; set; }

    // External Integration DbSets
    public DbSet<IntegrationConfig> IntegrationConfigs { get; set; }

    // Inventory & Purchasing Module DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<Rfq> Rfqs { get; set; }
    public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }

    // Sales & CRM DbSets
    public DbSet<Deal> Deals { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<DeliveryNote> DeliveryNotes { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesQuotation> SalesQuotations { get; set; }

    // Workflow Module DbSets
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowTask> WorkflowTasks { get; set; }

    // Finance & Expenses DbSets
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<ExpenseRequest> ExpenseRequests { get; set; }

    // Projects & Manufacturing DbSets
    public DbSet<Project> Projects { get; set; }
    public DbSet<BillOfMaterials> BillOfMaterials { get; set; }
    public DbSet<ManufacturingOrder> ManufacturingOrders { get; set; }

    // Fixed Assets & Maintenance DbSets
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

    // Payroll DbSets
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<Payslip> Payslips { get; set; }

    // Audit Log DbSets
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; }

    // Real-Time Chat & Notification DbSets
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<SystemNotification> SystemNotifications { get; set; }

    // Document Management System (DMS)
    public DbSet<Document> Documents { get; set; }
    public DbSet<Folder> Folders { get; set; }

    public ERPPlatformDbContext(DbContextOptions<ERPPlatformDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configure your own tables/entities inside here */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        // Unique indexes for Subscription System
        builder.Entity<Plan>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<Feature>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<PlanFeature>(b =>
        {
            b.HasIndex(x => new { x.PlanId, x.FeatureId }).IsUnique();
        });

        builder.Entity<UsageRecord>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.FeatureCode, x.PeriodStart, x.PeriodEnd }).IsUnique();
        });
    }
}
