using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;
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

    // Group Chat DbSets
    public DbSet<ChatConversation> ChatConversations { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<ChatMessageReaction> ChatMessageReactions { get; set; }

    // Document Management System (DMS)
    public DbSet<Document> Documents { get; set; }
    public DbSet<Folder> Folders { get; set; }

    // Mobile Support DbSets
    public DbSet<DeviceRegistration> DeviceRegistrations { get; set; }
    public DbSet<StockCount> StockCounts { get; set; }
    public DbSet<StockCountItem> StockCountItems { get; set; }
    public DbSet<PickList> PickLists { get; set; }
    public DbSet<PickListItem> PickListItems { get; set; }
    public DbSet<PackList> PackLists { get; set; }
    public DbSet<PackListItem> PackListItems { get; set; }
    public DbSet<FieldVisit> FieldVisits { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AiChatSession> AiChatSessions { get; set; }

    // Dashboard Customization DbSets
    public DbSet<DashboardWidget> DashboardWidgets { get; set; }
    public DbSet<UserDashboardConfig> UserDashboardConfigs { get; set; }

    // Leave Policy DbSets
    public DbSet<LeavePolicy> LeavePolicies { get; set; }

    // Recruitment, Execution History & Report Catalog DbSets
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<WorkflowExecutionLog> WorkflowExecutionLogs { get; set; }
    public DbSet<WorkflowExecutionStep> WorkflowExecutionSteps { get; set; }
    public DbSet<ReportDefinition> ReportDefinitions { get; set; }

    // Role Data Scope DbSets (per-role, per-page row-level scoping)
    public DbSet<RolePageScope> RolePageScopes { get; set; }

    // Resumable Employee Excel Import DbSets
    public DbSet<EmployeeImportJob> EmployeeImportJobs { get; set; }
    public DbSet<EmployeeImportChunk> EmployeeImportChunks { get; set; }
    public DbSet<EmployeeImportError> EmployeeImportErrors { get; set; }

    public ERPPlatformDbContext(DbContextOptions<ERPPlatformDbContext> options)
        : base(options)
    {

    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
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

        // Mobile Support Indexes
        builder.Entity<Product>(b =>
        {
            b.HasIndex(x => x.Barcode);
        });

        builder.Entity<DeviceRegistration>(b =>
        {
            b.HasIndex(x => x.DeviceToken);
            b.HasIndex(x => x.UserId);
        });

        builder.Entity<StockCountItem>(b =>
        {
            b.HasIndex(x => x.StockCountId);
        });

        builder.Entity<PickListItem>(b =>
        {
            b.HasIndex(x => x.PickListId);
        });

        builder.Entity<PackListItem>(b =>
        {
            b.HasIndex(x => x.PackListId);
        });

        builder.Entity<FieldVisit>(b =>
        {
            b.HasIndex(x => x.EmployeeId);
            b.HasIndex(x => x.CustomerId);
        });

        builder.Entity<Payment>(b =>
        {
            b.HasIndex(x => x.SalesInvoiceId);
        });

        builder.Entity<AiChatSession>(b =>
        {
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.SessionId);
        });

        // Dashboard Customization Indexes
        builder.Entity<DashboardWidget>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<UserDashboardConfig>(b =>
        {
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.DashboardName });
        });

        // Leave Policy Index
        builder.Entity<LeavePolicy>(b =>
        {
            b.HasIndex(x => x.LeaveType);
        });

        // Recruitment Indexes
        builder.Entity<Candidate>(b =>
        {
            b.HasIndex(x => x.Stage);
            b.HasIndex(x => x.AppliedPosition);
        });

        // Workflow Execution History Indexes
        builder.Entity<WorkflowExecutionLog>(b =>
        {
            b.HasIndex(x => x.WorkflowDefinitionId);
            b.HasIndex(x => x.Status);
        });

        builder.Entity<WorkflowExecutionStep>(b =>
        {
            b.HasIndex(x => x.WorkflowExecutionLogId);
        });

        // Report Catalog Index
        builder.Entity<ReportDefinition>(b =>
        {
            b.HasIndex(x => x.Category);
        });

        // Group Chat Indexes
        builder.Entity<ChatConversation>(b =>
        {
            b.HasIndex(x => x.Type);
            b.HasIndex(x => x.LastMessageAt);
            b.HasIndex(x => x.CreatorUserId);
        });

        builder.Entity<ChatParticipant>(b =>
        {
            b.HasIndex(x => x.ConversationId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.ConversationId, x.UserId });
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.HasIndex(x => x.ConversationId);
            b.HasIndex(x => x.ChannelName);
            b.HasIndex(x => x.SenderId);
            b.HasIndex(x => new { x.ConversationId, x.Timestamp });
        });

        builder.Entity<ChatMessageReaction>(b =>
        {
            b.HasIndex(x => x.MessageId);
            b.HasIndex(x => new { x.MessageId, x.UserId, x.Emoji });
        });

        // Role Data Scope: one scope row per role per page.
        builder.Entity<RolePageScope>(b =>
        {
            b.HasIndex(x => new { x.RoleName, x.PageKey });
            b.HasIndex(x => x.PageKey);
        });

        // ── Employee Excel Import ────────────────────────────────
        builder.Entity<EmployeeImportJob>(b =>
        {
            b.ToTable(ERPPlatformConsts.DbTablePrefix + "EmployeeImportJobs", ERPPlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(512);
            b.Property(x => x.StorageKey).IsRequired().HasMaxLength(1024);
            b.Property(x => x.FileHash).HasMaxLength(128);
            // "Same user submitting the same file while it is still running" is the
            // duplicate-submission guard the UI relies on.
            b.HasIndex(x => new { x.CreatorId, x.FileHash, x.Status });
            b.Property(x => x.CreatedByUserName).HasMaxLength(256);
            b.Property(x => x.LastError).HasMaxLength(EmployeeImportConsts.MaxErrorMessageLength);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => x.CreatedByUserName);
        });

        builder.Entity<EmployeeImportChunk>(b =>
        {
            b.ToTable(ERPPlatformConsts.DbTablePrefix + "EmployeeImportChunks", ERPPlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.LastError).HasMaxLength(EmployeeImportConsts.MaxErrorMessageLength);
            b.Property(x => x.HangfireJobId).HasMaxLength(128);

            // One chunk row per (job, chunk number) — the natural key the resume
            // logic uses, and the guard against duplicate chunk creation.
            b.HasIndex(x => new { x.ImportJobId, x.ChunkNumber }).IsUnique();

            // Speeds up "find the next chunk that still needs work".
            b.HasIndex(x => new { x.ImportJobId, x.Status });
        });

        builder.Entity<EmployeeImportError>(b =>
        {
            b.ToTable(ERPPlatformConsts.DbTablePrefix + "EmployeeImportErrors", ERPPlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ColumnName).HasMaxLength(128);
            b.Property(x => x.Value).HasMaxLength(EmployeeImportConsts.MaxErrorValueLength);
            b.Property(x => x.ErrorMessage).IsRequired().HasMaxLength(EmployeeImportConsts.MaxErrorMessageLength);
            b.HasIndex(x => x.ImportJobId);
            b.HasIndex(x => x.ChunkId);
        });
    }
}
