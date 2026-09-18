using System;
using System.Collections.Generic;
using ERPPlatform.Application.Imports;
using ERPPlatform.Imports;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.Imports;

public class EmployeeImportRowValidatorTests
{
    private readonly EmployeeImportRowValidator _validator = new();
    private readonly List<string> _knownDepartments = new() { "Engineering", "Human Resources", "Sales", "Finance" };

    [Fact]
    public void ValidRow_PassesValidation_WithParsedFields()
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 2,
            EmployeeCode = "EMP-001",
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "+201012345678",
            Position = "Senior Software Engineer",
            DepartmentName = "Engineering",
            Salary = "25000",
            JoiningDate = "2026-01-15",
            Status = "Active",
            Location = "Cairo HQ",
            LeaveBalance = "21"
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeTrue();
        outcome.Errors.ShouldBeEmpty();
        outcome.Parsed.ShouldNotBeNull();
        outcome.Parsed.EmployeeCode.ShouldBe("EMP-001");
        outcome.Parsed.Name.ShouldBe("John Doe");
        outcome.Parsed.Email.ShouldBe("john.doe@example.com");
        outcome.Parsed.Salary.ShouldBe(25000m);
        outcome.Parsed.DepartmentName.ShouldBe("Engineering");
    }

    [Fact]
    public void MissingRequiredFields_ReturnsValidationErrors()
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 3,
            EmployeeCode = "",
            Name = "",
            Email = ""
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeFalse();
        outcome.Errors.Count.ShouldBeGreaterThanOrEqualTo(3);
        outcome.Parsed.ShouldBeNull();
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnEmployeeCode);
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnName);
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnEmail);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    public void InvalidEmail_FailsValidation(string badEmail)
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 4,
            EmployeeCode = "EMP-002",
            Name = "Jane Doe",
            Email = badEmail
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeFalse();
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnEmail);
    }

    [Fact]
    public void NegativeSalary_FailsValidation()
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 5,
            EmployeeCode = "EMP-003",
            Name = "Bob Smith",
            Email = "bob@example.com",
            Salary = "-500"
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeFalse();
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnSalary);
    }

    [Fact]
    public void UnknownDepartment_FailsValidation_WhenDepartmentsAreProvided()
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 6,
            EmployeeCode = "EMP-004",
            Name = "Alice Green",
            Email = "alice@example.com",
            DepartmentName = "NonExistentDept"
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeFalse();
        outcome.Errors.ShouldContain(e => e.ColumnName == EmployeeImportConsts.ColumnDepartmentName);
    }

    [Fact]
    public void AllowedStatus_CaseInsensitive_PassesValidation()
    {
        var row = new EmployeeImportRow
        {
            RowNumber = 7,
            EmployeeCode = "EMP-005",
            Name = "Mark Brown",
            Email = "mark@example.com",
            Status = "active"
        };

        var outcome = _validator.Validate(row, _knownDepartments);

        outcome.IsValid.ShouldBeTrue();
        outcome.Parsed.ShouldNotBeNull();
        outcome.Parsed.Status.ShouldBe("Active");
    }
}
