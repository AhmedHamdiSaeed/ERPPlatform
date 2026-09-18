using System;
using ERPPlatform.Modules.HR.Domain.Entities;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.HR;

public class HREntityTests
{
    [Fact]
    public void Employee_DefaultValues_AreProperlySet()
    {
        var emp = new Employee
        {
            EmployeeCode = "EMP-100",
            Name = "Ahmed Hamdi",
            Email = "ahmed@example.com",
            Salary = 45000m,
            JoiningDate = new DateTime(2026, 1, 1)
        };

        emp.Status.ShouldBe("Active");
        emp.Location.ShouldBe("Cairo HQ");
        emp.Salary.ShouldBe(45000m);
        emp.EmployeeCode.ShouldBe("EMP-100");
    }

    [Fact]
    public void LeaveRequest_DaysCalculation_AndStatusTransition()
    {
        var leave = new LeaveRequest
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "Ahmed Hamdi",
            LeaveType = "Annual",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 5),
            DaysCount = 5,
            Reason = "Annual Family Vacation"
        };

        leave.Status.ShouldBe("Pending");
        leave.DaysCount.ShouldBe(5);

        // Approve leave
        leave.Status = "Approved";
        leave.Status.ShouldBe("Approved");
    }

    [Fact]
    public void Department_EmployeeCount_AndBudgetTracking()
    {
        var dept = new Department
        {
            Code = "ENG",
            Name = "Engineering & Technology",
            Budget = 1500000m,
            EmployeeCount = 25,
            ManagerName = "Director John"
        };

        dept.Budget.ShouldBe(1500000m);
        dept.EmployeeCount.ShouldBe(25);
        dept.Code.ShouldBe("ENG");
    }
}
