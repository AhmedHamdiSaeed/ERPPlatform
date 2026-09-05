using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ERPPlatform.Imports;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Application.Imports;

/// <summary>Validated, typed form of a spreadsheet row.</summary>
public class EmployeeImportParsedRow
{
    public int RowNumber { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime JoiningDate { get; set; }
    public string Status { get; set; } = "Active";
    public string Location { get; set; } = "Cairo HQ";
    public string ManagerName { get; set; } = string.Empty;
    public decimal LeaveBalance { get; set; } = 21.0m;
}

/// <summary>
/// Business-rule validation for one row. A rejected row is recorded and the chunk
/// carries on, so a single typo cannot sink a 10,000-row import.
/// </summary>
public interface IEmployeeImportRowValidator
{
    ValidationOutcome Validate(EmployeeImportRow row, IEnumerable<string> knownDepartmentNames);
}

public class ValidationOutcome
{
    public bool IsValid => Errors.Count == 0;

    public EmployeeImportParsedRow? Parsed { get; set; }

    public List<EmployeeImportRowError> Errors { get; set; } = new();
}

public class EmployeeImportRowValidator : IEmployeeImportRowValidator, ITransientDependency
{
    /// <summary>Mirrors the values accepted by the employee UI. Anything else is rejected.</summary>
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Active", "On Leave", "Inactive" };

    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "MM/dd/yyyy",
        "dd-MM-yyyy", "MM-dd-yyyy", "yyyy.MM.dd", "d/M/yyyy", "M/d/yyyy",
        "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss"
    };

    public ValidationOutcome Validate(EmployeeImportRow row, IEnumerable<string> knownDepartmentNames)
    {
        var outcome = new ValidationOutcome
        {
            Parsed = new EmployeeImportParsedRow { RowNumber = row.RowNumber }
        };

        void Fail(string column, string? value, string message)
        {
            outcome.Errors.Add(new EmployeeImportRowError
            {
                RowNumber = row.RowNumber,
                ColumnName = column,
                Value = Clamp(value),
                ErrorMessage = message
            });
        }

        // ── Employee code ───────────────────────────────────────
        var code = row.EmployeeCode.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            Fail(EmployeeImportConsts.ColumnEmployeeCode, code, "Employee number is required.");
        }
        else if (code.Length > 64)
        {
            Fail(EmployeeImportConsts.ColumnEmployeeCode, code, "Employee number cannot be longer than 64 characters.");
        }
        else
        {
            outcome.Parsed!.EmployeeCode = code;
        }

        // ── Name ────────────────────────────────────────────────
        var name = row.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            Fail(EmployeeImportConsts.ColumnName, name, "Name is required.");
        }
        else if (name.Length > 256)
        {
            Fail(EmployeeImportConsts.ColumnName, name, "Name cannot be longer than 256 characters.");
        }
        else
        {
            outcome.Parsed!.Name = name;
        }

        // ── Email ───────────────────────────────────────────────
        var email = row.Email.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            Fail(EmployeeImportConsts.ColumnEmail, email, "Email is required.");
        }
        else if (!IsValidEmail(email))
        {
            Fail(EmployeeImportConsts.ColumnEmail, email, "Invalid email address");
        }
        else if (email.Length > 256)
        {
            Fail(EmployeeImportConsts.ColumnEmail, email, "Email cannot be longer than 256 characters.");
        }
        else
        {
            outcome.Parsed!.Email = email;
        }

        // ── Optional fields (validated only when supplied) ───────
        if (!string.IsNullOrWhiteSpace(row.Phone))
        {
            if (row.Phone.Trim().Length > 32)
            {
                Fail(EmployeeImportConsts.ColumnPhone, row.Phone, "Phone cannot be longer than 32 characters.");
            }
            else
            {
                outcome.Parsed!.Phone = row.Phone.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(row.Position))
        {
            if (row.Position.Trim().Length > 128)
            {
                Fail(EmployeeImportConsts.ColumnPosition, row.Position, "Job title cannot be longer than 128 characters.");
            }
            else
            {
                outcome.Parsed!.Position = row.Position.Trim();
            }
        }

        var department = row.DepartmentName.Trim();
        if (!string.IsNullOrWhiteSpace(department))
        {
            if (department.Length > 128)
            {
                Fail(EmployeeImportConsts.ColumnDepartmentName, department, "Department cannot be longer than 128 characters.");
            }
            else if (!MatchesKnownDepartment(department, knownDepartmentNames))
            {
                Fail(EmployeeImportConsts.ColumnDepartmentName, department, "Invalid department");
            }
            else
            {
                outcome.Parsed!.DepartmentName = department;
            }
        }

        if (!string.IsNullOrWhiteSpace(row.Salary))
        {
            if (!TryParseDecimal(row.Salary, out var salary) || salary < 0)
            {
                Fail(EmployeeImportConsts.ColumnSalary, row.Salary, "Salary must be a non-negative number.");
            }
            else
            {
                outcome.Parsed!.Salary = salary;
            }
        }

        if (!string.IsNullOrWhiteSpace(row.JoiningDate))
        {
            if (!TryParseDate(row.JoiningDate, out var joiningDate))
            {
                Fail(EmployeeImportConsts.ColumnJoiningDate, row.JoiningDate, "Joining date must be a valid date (e.g. 2026-09-05).");
            }
            else
            {
                outcome.Parsed!.JoiningDate = joiningDate;
            }
        }
        else
        {
            // Same default the manual create form uses.
            outcome.Parsed!.JoiningDate = DateTime.UtcNow.Date;
        }

        var status = row.Status.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!AllowedStatuses.Contains(status))
            {
                Fail(EmployeeImportConsts.ColumnStatus, status,
                    $"Invalid status. Allowed values: {string.Join(", ", AllowedStatuses)}.");
            }
            else
            {
                outcome.Parsed!.Status = ToTitleStatus(status);
            }
        }

        if (!string.IsNullOrWhiteSpace(row.Location))
        {
            outcome.Parsed!.Location = row.Location.Trim();
        }

        if (!string.IsNullOrWhiteSpace(row.ManagerName))
        {
            outcome.Parsed!.ManagerName = row.ManagerName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(row.LeaveBalance))
        {
            if (!TryParseDecimal(row.LeaveBalance, out var balance) || balance < 0)
            {
                Fail(EmployeeImportConsts.ColumnLeaveBalance, row.LeaveBalance, "Leave balance must be a non-negative number.");
            }
            else
            {
                outcome.Parsed!.LeaveBalance = balance;
            }
        }

        if (!outcome.IsValid)
        {
            outcome.Parsed = null;
        }

        return outcome;
    }

    /// <summary>
    /// A department is accepted when it matches an existing department (by name or
    /// code). If the tenant has no departments at all the column is free text —
    /// refusing it would make the import unusable on a fresh database.
    /// </summary>
    private static bool MatchesKnownDepartment(string department, IEnumerable<string> knownDepartmentNames)
    {
        var known = knownDepartmentNames?.ToList() ?? new List<string>();
        if (known.Count == 0) return true;

        return known.Any(d =>
            string.Equals(d, department, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidEmail(string email)
    {
        // Deliberately stricter than MailAddress: require a dot in the domain.
        var at = email.LastIndexOf('@');
        if (at <= 0 || at == email.Length - 1) return false;

        var domain = email.Substring(at + 1);
        if (!domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.')) return false;
        if (email.Contains(' ')) return false;

        return email.Count(c => c == '@') == 1;
    }

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        var text = raw.Trim().Replace(",", string.Empty).Replace(" ", string.Empty);

        // Tolerate a trailing currency symbol such as "18000 EGP".
        while (text.Length > 0 && !char.IsDigit(text[^1]) && text[^1] != '.')
        {
            text = text[..^1];
        }

        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value) ||
               decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    private static bool TryParseDate(string raw, out DateTime value)
    {
        value = default;

        // Excel stores dates as serial numbers; DataFormatter already renders most
        // of them, but a raw numeric cell can still reach here.
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial) &&
            serial > 0 && serial < 2958466)
        {
            value = new DateTime(1899, 12, 30).AddDays(serial);
            return true;
        }

        return DateTime.TryParseExact(raw.Trim(), DateFormats, CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out value)
               || DateTime.TryParse(raw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
               || DateTime.TryParse(raw.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out value);
    }

    private static string ToTitleStatus(string status)
    {
        return AllowedStatuses.First(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Clamp(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var trimmed = value.Trim();
        return trimmed.Length <= EmployeeImportConsts.MaxErrorValueLength
            ? trimmed
            : trimmed.Substring(0, EmployeeImportConsts.MaxErrorValueLength);
    }
}
