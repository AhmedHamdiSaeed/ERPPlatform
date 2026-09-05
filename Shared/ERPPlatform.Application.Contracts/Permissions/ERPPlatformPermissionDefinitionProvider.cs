using ERPPlatform.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ERPPlatform.Permissions;

/// <summary>
/// Defines one parent permission per page (the 'View' right) plus Create / Edit / Delete
/// children. Page keys match the Angular permission-catalog so the Roles & Permissions UI
/// can toggle them 1:1. Data scoping is handled separately by RolePageScope.
/// </summary>
public class ERPPlatformPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ERPPlatformPermissions.GroupName, L("Permission:GroupName"));

        DefinePage(myGroup, "ERPPlatform.Employees");
        DefinePage(myGroup, "ERPPlatform.Attendance");
        DefinePage(myGroup, "ERPPlatform.LeaveRequests");
        DefinePage(myGroup, "ERPPlatform.Payroll");
        DefinePage(myGroup, "ERPPlatform.Payslips");
        DefinePage(myGroup, "ERPPlatform.Departments");
        DefinePage(myGroup, "ERPPlatform.Positions");
        DefinePage(myGroup, "ERPPlatform.Contracts");
        DefinePage(myGroup, "ERPPlatform.Recruitment");
        DefinePage(myGroup, "ERPPlatform.Customers");
        DefinePage(myGroup, "ERPPlatform.Leads");
        DefinePage(myGroup, "ERPPlatform.Deals");
        DefinePage(myGroup, "ERPPlatform.SalesOrders");
        DefinePage(myGroup, "ERPPlatform.SalesQuotations");
        DefinePage(myGroup, "ERPPlatform.Invoices");
        DefinePage(myGroup, "ERPPlatform.DeliveryNotes");
        DefinePage(myGroup, "ERPPlatform.Payments");
        DefinePage(myGroup, "ERPPlatform.Products");
        DefinePage(myGroup, "ERPPlatform.Warehouses");
        DefinePage(myGroup, "ERPPlatform.StockTransfers");
        DefinePage(myGroup, "ERPPlatform.PurchaseOrders");
        DefinePage(myGroup, "ERPPlatform.PurchaseRequests");
        DefinePage(myGroup, "ERPPlatform.GoodsReceipts");
        DefinePage(myGroup, "ERPPlatform.Suppliers");
        DefinePage(myGroup, "ERPPlatform.Accounts");
        DefinePage(myGroup, "ERPPlatform.JournalEntries");
        DefinePage(myGroup, "ERPPlatform.ExpenseRequests");
        DefinePage(myGroup, "ERPPlatform.Projects");
        DefinePage(myGroup, "ERPPlatform.ManufacturingOrders");
        DefinePage(myGroup, "ERPPlatform.FixedAssets");
        DefinePage(myGroup, "ERPPlatform.MaintenanceRequests");
        DefinePage(myGroup, "ERPPlatform.WorkflowDefinitions");
        DefinePage(myGroup, "ERPPlatform.WorkflowTasks");
        DefinePage(myGroup, "ERPPlatform.ApprovalCenter");
        DefinePage(myGroup, "ERPPlatform.ReportDefinitions");
        DefinePage(myGroup, "ERPPlatform.Documents");
        DefinePage(myGroup, "ERPPlatform.Dashboard");
        DefinePage(myGroup, "ERPPlatform.Users");
        DefinePage(myGroup, "ERPPlatform.Roles");
        DefinePage(myGroup, "ERPPlatform.Companies");
        DefinePage(myGroup, "ERPPlatform.Branches");
        DefinePage(myGroup, "ERPPlatform.Currencies");
        DefinePage(myGroup, "ERPPlatform.IntegrationConfigs");
        DefinePage(myGroup, "ERPPlatform.Settings");

        DefineEmployeeImport(myGroup);
    }

    /// <summary>
    /// Employee import needs more than View/Create/Edit/Delete, so it is defined
    /// explicitly. Names mirror <see cref="ERPPlatformPermissions.EmployeeImport"/>.
    /// </summary>
    private static void DefineEmployeeImport(PermissionGroupDefinition group)
    {
        var page = group.AddPermission(ERPPlatformPermissions.EmployeeImport.Default, L("Permission:EmployeeImport"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.View, L("Permission:EmployeeImport:View"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.Create, L("Permission:EmployeeImport:Create"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.Retry, L("Permission:EmployeeImport:Retry"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.Cancel, L("Permission:EmployeeImport:Cancel"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.ViewErrors, L("Permission:EmployeeImport:ViewErrors"));
        page.AddChild(ERPPlatformPermissions.EmployeeImport.Delete, L("Permission:EmployeeImport:Delete"));
    }

    private static void DefinePage(PermissionGroupDefinition group, string key)
    {
        var baseKey = "Permission:" + key["ERPPlatform.".Length..];
        var page = group.AddPermission(key, L(baseKey));
        page.AddChild(key + ".Create", L(baseKey + ":Create"));
        page.AddChild(key + ".Edit", L(baseKey + ":Edit"));
        page.AddChild(key + ".Delete", L(baseKey + ":Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ERPPlatformResource>(name);
    }
}
