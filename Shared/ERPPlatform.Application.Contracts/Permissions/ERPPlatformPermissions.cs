namespace ERPPlatform.Permissions;

public static class ERPPlatformPermissions
{
    public const string GroupName = "ERPPlatform";

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
        public const string View = Default + ".View";
    }

    public static class Users
    {
        public const string Default = GroupName + ".Users";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Roles
    {
        public const string Default = GroupName + ".Roles";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Invoices
    {
        public const string Default = GroupName + ".Invoices";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
    }

    /// <summary>
    /// Employee Excel import. <c>View</c> doubles as the page permission for the
    /// Import History page; <c>Create</c> gates starting a new import.
    /// </summary>
    public static class EmployeeImport
    {
        public const string Default = GroupName + ".EmployeeImport";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Retry = Default + ".Retry";
        public const string Cancel = Default + ".Cancel";
        public const string ViewErrors = Default + ".ViewErrors";
        public const string Delete = Default + ".Delete";
    }
}
