using ERPPlatform.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ERPPlatform.Permissions;

public class ERPPlatformPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ERPPlatformPermissions.GroupName, L("Permission:GroupName"));

        var dashboard = myGroup.AddPermission(ERPPlatformPermissions.Dashboard.Default, L("Permission:Dashboard"));
        dashboard.AddChild(ERPPlatformPermissions.Dashboard.View, L("Permission:Dashboard:View"));

        var users = myGroup.AddPermission(ERPPlatformPermissions.Users.Default, L("Permission:Users"));
        users.AddChild(ERPPlatformPermissions.Users.Create, L("Permission:Users:Create"));
        users.AddChild(ERPPlatformPermissions.Users.Edit, L("Permission:Users:Edit"));
        users.AddChild(ERPPlatformPermissions.Users.Delete, L("Permission:Users:Delete"));

        var roles = myGroup.AddPermission(ERPPlatformPermissions.Roles.Default, L("Permission:Roles"));
        roles.AddChild(ERPPlatformPermissions.Roles.Create, L("Permission:Roles:Create"));
        roles.AddChild(ERPPlatformPermissions.Roles.Edit, L("Permission:Roles:Edit"));
        roles.AddChild(ERPPlatformPermissions.Roles.Delete, L("Permission:Roles:Delete"));

        var customers = myGroup.AddPermission(ERPPlatformPermissions.Customers.Default, L("Permission:Customers"));
        customers.AddChild(ERPPlatformPermissions.Customers.Create, L("Permission:Customers:Create"));
        customers.AddChild(ERPPlatformPermissions.Customers.Edit, L("Permission:Customers:Edit"));
        customers.AddChild(ERPPlatformPermissions.Customers.Delete, L("Permission:Customers:Delete"));

        var invoices = myGroup.AddPermission(ERPPlatformPermissions.Invoices.Default, L("Permission:Invoices"));
        invoices.AddChild(ERPPlatformPermissions.Invoices.Create, L("Permission:Invoices:Create"));
        invoices.AddChild(ERPPlatformPermissions.Invoices.Edit, L("Permission:Invoices:Edit"));
        invoices.AddChild(ERPPlatformPermissions.Invoices.Delete, L("Permission:Invoices:Delete"));

        myGroup.AddPermission(ERPPlatformPermissions.Settings.Default, L("Permission:Settings"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ERPPlatformResource>(name);
    }
}
