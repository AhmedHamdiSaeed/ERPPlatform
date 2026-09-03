namespace ERPPlatform.Application.RoleScopes;

/// <summary>
/// Stable identifiers for pages that support data scoping. These are the keys stored in
/// RolePageScopes.PageKey and must stay in sync with the page keys defined in the Angular
/// roles UI (angular/src/app/core/models/permission-catalog.ts).
/// </summary>
public static class DataScopePageKeys
{
    public const string Employees = "ERPPlatform.Employees";
    public const string Attendance = "ERPPlatform.Attendance";
    public const string LeaveRequests = "ERPPlatform.LeaveRequests";
    public const string Payroll = "ERPPlatform.Payroll";
    public const string Payslips = "ERPPlatform.Payslips";
}
