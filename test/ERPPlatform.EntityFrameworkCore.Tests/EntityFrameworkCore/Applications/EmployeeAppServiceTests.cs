using System;
using System.Threading.Tasks;
using ERPPlatform.Modules.HR.Application;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications;

/// <summary>
/// Integration tests for EmployeeAppService against an in-memory SQLite database.
/// Covers CRUD operations, filtering, and domain-level invariants.
/// </summary>
public class EmployeeAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase<ERPPlatformEntityFrameworkCoreTestModule>
{
    private readonly IEmployeeAppService _employeeService;

    public EmployeeAppServiceTests()
    {
        _employeeService = GetRequiredService<IEmployeeAppService>();
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_Should_Persist_Employee_With_Correct_Values()
    {
        var input = new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-001",
            Name = "Sara Khalil",
            Email = "sara.khalil@erp.com",
            Phone = "+20 100 555 1234",
            Position = "Backend Developer",
            DepartmentName = "Engineering",
            Salary = 72_000m,
            Status = "Active"
        };

        var result = await _employeeService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Sara Khalil");
        result.Email.ShouldBe("sara.khalil@erp.com");
        result.EmployeeCode.ShouldBe("EMP-TEST-001");
        result.Status.ShouldBe("Active");
        result.Salary.ShouldBe(72_000m);
    }

    [Fact]
    public async Task CreateAsync_Should_Default_Status_To_Active()
    {
        var input = new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-002",
            Name = "Omar Nasser",
            Email = "omar.nasser@erp.com",
            Position = "Data Analyst",
            DepartmentName = "Analytics"
        };

        var result = await _employeeService.CreateAsync(input);
        result.Status.ShouldBe("Active");
    }

    // ─── READ ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_Should_Return_Created_Employee()
    {
        var created = await _employeeService.CreateAsync(new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-003",
            Name = "Layla Hassan",
            Email = "layla@erp.com",
            Position = "UX Designer",
            DepartmentName = "Design",
            Status = "Active"
        });

        var fetched = await _employeeService.GetAsync(created.Id);

        fetched.ShouldNotBeNull();
        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe("Layla Hassan");
    }

    [Fact]
    public async Task GetListAsync_Should_Return_All_Employees()
    {
        await _employeeService.CreateAsync(new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-010",
            Name = "Ahmed Ali",
            Email = "ahmed.ali@erp.com",
            Position = "Manager",
            DepartmentName = "HR"
        });
        await _employeeService.CreateAsync(new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-011",
            Name = "Nadia Said",
            Email = "nadia.said@erp.com",
            Position = "Recruiter",
            DepartmentName = "HR"
        });

        var list = await _employeeService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_Should_Change_Position_And_Salary()
    {
        var created = await _employeeService.CreateAsync(new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-020",
            Name = "Khaled Mostafa",
            Email = "khaled@erp.com",
            Position = "Junior Developer",
            DepartmentName = "Engineering",
            Salary = 45_000m,
            Status = "Active"
        });

        var updateDto = new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-020",
            Name = "Khaled Mostafa",
            Email = "khaled@erp.com",
            Position = "Senior Developer",
            DepartmentName = "Engineering",
            Salary = 90_000m,
            Status = "Active"
        };

        var updated = await _employeeService.UpdateAsync(created.Id, updateDto);

        updated.Position.ShouldBe("Senior Developer");
        updated.Salary.ShouldBe(90_000m);
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_Should_Remove_Employee_From_Repository()
    {
        var created = await _employeeService.CreateAsync(new CreateUpdateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-030",
            Name = "Fatima Ibrahim",
            Email = "fatima@erp.com",
            Position = "Accountant",
            DepartmentName = "Finance",
            Status = "Active"
        });

        await _employeeService.DeleteAsync(created.Id);

        await Should.ThrowAsync<Exception>(async () =>
            await _employeeService.GetAsync(created.Id));
    }
}
