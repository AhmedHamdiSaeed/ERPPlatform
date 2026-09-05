using System.Collections.Generic;

namespace ERPPlatform.Imports;

/// <summary>
/// Limits and the spreadsheet contract for the employee Excel import.
/// The column names are the *canonical* headers; the parser matches them
/// case-insensitively and also accepts a few common aliases so real-world dumps
/// (e.g. "Emp Code", "E-mail") still import.
/// </summary>
public static class EmployeeImportConsts
{
    public const int MaxErrorMessageLength = 2000;
    public const int MaxErrorValueLength = 512;

    /// <summary>Default rows per chunk when <c>EmployeeImport:ChunkSize</c> is not configured.</summary>
    public const int DefaultChunkSize = 100;

    /// <summary>Hard ceiling on rows per chunk, so config cannot accidentally create one giant chunk.</summary>
    public const int MaxChunkSize = 5000;

    /// <summary>
    /// Hard ceiling on the uploaded file (25 MB). The parser loads the workbook into
    /// memory to support both .xlsx and .xls; raising this a lot needs the streaming
    /// reader instead. Kestrel's <c>MaxRequestBodySize</c> must be raised in step.
    /// </summary>
    public const long MaxFileSizeBytes = 25L * 1024 * 1024;

    /// <summary>Hard ceiling on data rows in a single file.</summary>
    public const int MaxTotalRows = 100_000;

    /// <summary>How many times a single chunk may be attempted before the job is marked Failed.</summary>
    public const int MaxChunkAttempts = 5;

    public const string DefaultSheetBehavior = "FirstSheet";

    /// <summary>Extensions the importer accepts. Mirrors the generic file-import controller allow-list.</summary>
    public static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

    /// <summary>Blob container that holds retained source files until an import finishes.</summary>
    public const string BlobContainerName = "employee-imports";

    // ── Spreadsheet contract ────────────────────────────────────
    // Required columns: the import cannot run without them.
    public const string ColumnEmployeeCode = "EmployeeCode";
    public const string ColumnName = "Name";
    public const string ColumnEmail = "Email";

    // Optional columns: imported when present.
    public const string ColumnPhone = "Phone";
    public const string ColumnPosition = "Position";
    public const string ColumnDepartmentName = "DepartmentName";
    public const string ColumnSalary = "Salary";
    public const string ColumnJoiningDate = "JoiningDate";
    public const string ColumnStatus = "Status";
    public const string ColumnLocation = "Location";
    public const string ColumnManagerName = "ManagerName";
    public const string ColumnLeaveBalance = "LeaveBalance";

    /// <summary>
    /// Accepted header spellings for each canonical column. Matching is
    /// case-insensitive and ignores spaces, underscores and hyphens.
    /// </summary>
    public static readonly Dictionary<string, string[]> ColumnAliases = new()
    {
        [ColumnEmployeeCode] = new[] { "Employee Code", "EmployeeCode", "Employee No", "EmployeeNumber", "Emp Code", "Code", "Employee ID", "EmployeeId", "StaffId", "Staff ID" },
        [ColumnName] = new[] { "Name", "Full Name", "FullName", "Employee Name", "EmployeeName", "Employee" },
        [ColumnEmail] = new[] { "Email", "E-mail", "Email Address", "EmailAddress", "Mail", "Work Email" },
        [ColumnPhone] = new[] { "Phone", "Phone Number", "PhoneNumber", "Mobile", "Mobile Number", "Telephone", "Contact" },
        [ColumnPosition] = new[] { "Position", "Job Title", "JobTitle", "Title", "Role", "Designation" },
        [ColumnDepartmentName] = new[] { "Department", "Department Name", "DepartmentName", "Dept", "Department Code", "DepartmentCode" },
        [ColumnSalary] = new[] { "Salary", "Basic Salary", "BaseSalary", "Monthly Salary", "Gross Salary" },
        [ColumnJoiningDate] = new[] { "Joining Date", "JoiningDate", "Join Date", "JoinDate", "Hire Date", "HireDate", "Start Date", "StartDate" },
        [ColumnStatus] = new[] { "Status", "Employment Status", "EmploymentStatus", "Employee Status" },
        [ColumnLocation] = new[] { "Location", "Branch", "Work Location", "WorkLocation", "Site" },
        [ColumnManagerName] = new[] { "Manager", "Manager Name", "ManagerName", "Reports To", "ReportsTo", "Supervisor" },
        [ColumnLeaveBalance] = new[] { "Leave Balance", "LeaveBalance", "Annual Leave Balance", "Vacation Balance" }
    };

    /// <summary>The columns that must be present in the header row.</summary>
    public static readonly string[] RequiredColumns = { ColumnEmployeeCode, ColumnName, ColumnEmail };

    /// <summary>Every recognised column, in the order used for error reporting.</summary>
    public static readonly string[] AllColumns =
    {
        ColumnEmployeeCode, ColumnName, ColumnEmail, ColumnPhone, ColumnPosition,
        ColumnDepartmentName, ColumnSalary, ColumnJoiningDate, ColumnStatus,
        ColumnLocation, ColumnManagerName, ColumnLeaveBalance
    };
}
