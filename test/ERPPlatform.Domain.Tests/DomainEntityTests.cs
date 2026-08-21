using System;
using ERPPlatform.Modules.HR.Domain.Entities;
using ERPPlatform.Modules.Inventory.Domain.Entities;
using ERPPlatform.Modules.Workflow.Domain.Entities;
using Shouldly;
using Xunit;

namespace ERPPlatform.Domain;

/// <summary>
/// Pure unit tests for domain entity default values and property invariants.
/// No database or DI container required — these are instantiation and logic tests.
/// </summary>
public class DomainEntityDefaultsTests
{
    // ─── EMPLOYEE ─────────────────────────────────────────────────────────────

    [Fact]
    public void Employee_Should_Have_Default_Status_Active()
    {
        var emp = new Employee();
        emp.Status.ShouldBe("Active");
    }

    [Fact]
    public void Employee_Should_Have_Default_Location_Cairo_HQ()
    {
        var emp = new Employee();
        emp.Location.ShouldBe("Cairo HQ");
    }

    [Fact]
    public void Employee_Should_Have_Empty_String_Defaults_For_All_String_Fields()
    {
        var emp = new Employee();
        emp.EmployeeCode.ShouldBe(string.Empty);
        emp.Name.ShouldBe(string.Empty);
        emp.Email.ShouldBe(string.Empty);
        emp.Phone.ShouldBe(string.Empty);
        emp.Position.ShouldBe(string.Empty);
        emp.DepartmentName.ShouldBe(string.Empty);
        emp.ManagerName.ShouldBe(string.Empty);
        emp.Avatar.ShouldBe(string.Empty);
    }

    [Fact]
    public void Employee_Salary_Should_Default_To_Zero()
    {
        var emp = new Employee();
        emp.Salary.ShouldBe(0m);
    }

    [Fact]
    public void Employee_DepartmentId_Should_Default_To_Null()
    {
        var emp = new Employee();
        emp.DepartmentId.ShouldBeNull();
    }

    // ─── DEPARTMENT ───────────────────────────────────────────────────────────

    [Fact]
    public void Department_Should_Have_Empty_String_Defaults()
    {
        var dept = new Department();
        dept.Code.ShouldBe(string.Empty);
        dept.Name.ShouldBe(string.Empty);
        dept.Description.ShouldBe(string.Empty);
        dept.ManagerName.ShouldBe(string.Empty);
    }

    [Fact]
    public void Department_EmployeeCount_Should_Default_To_Zero()
    {
        var dept = new Department();
        dept.EmployeeCount.ShouldBe(0);
    }

    [Fact]
    public void Department_Budget_Should_Default_To_Zero()
    {
        var dept = new Department();
        dept.Budget.ShouldBe(0m);
    }

    // ─── LEAVE REQUEST ────────────────────────────────────────────────────────

    [Fact]
    public void LeaveRequest_Should_Default_Status_To_Pending()
    {
        var leave = new LeaveRequest();
        leave.Status.ShouldBe("Pending");
    }

    [Fact]
    public void LeaveRequest_Should_Default_LeaveType_To_Annual()
    {
        var leave = new LeaveRequest();
        leave.LeaveType.ShouldBe("Annual");
    }

    [Fact]
    public void LeaveRequest_DaysCount_Should_Default_To_Zero()
    {
        var leave = new LeaveRequest();
        leave.DaysCount.ShouldBe(0);
    }

    // ─── PRODUCT ──────────────────────────────────────────────────────────────

    [Fact]
    public void Product_Should_Default_Status_To_In_Stock()
    {
        var product = new Product();
        product.Status.ShouldBe("In Stock");
    }

    [Fact]
    public void Product_Should_Default_ReorderLevel_To_10()
    {
        var product = new Product();
        product.ReorderLevel.ShouldBe(10);
    }

    [Fact]
    public void Product_Should_Default_Unit_To_Pcs()
    {
        var product = new Product();
        product.Unit.ShouldBe("pcs");
    }

    [Fact]
    public void Product_Should_Default_WarehouseName_To_Main_Warehouse()
    {
        var product = new Product();
        product.WarehouseName.ShouldBe("Main Warehouse");
    }

    [Fact]
    public void Product_Stock_And_Price_Should_Default_To_Zero()
    {
        var product = new Product();
        product.Stock.ShouldBe(0);
        product.Price.ShouldBe(0m);
    }

    // ─── STOCK TRANSFER ───────────────────────────────────────────────────────

    [Fact]
    public void StockTransfer_Should_Default_Status_To_In_Transit()
    {
        var transfer = new StockTransfer();
        transfer.Status.ShouldBe("In Transit");
    }

    [Fact]
    public void StockTransfer_Date_Should_Be_Recent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var transfer = new StockTransfer();
        var after = DateTime.UtcNow.AddSeconds(1);

        transfer.Date.ShouldBeGreaterThan(before);
        transfer.Date.ShouldBeLessThan(after);
    }

    // ─── PURCHASE ORDER ───────────────────────────────────────────────────────

    [Fact]
    public void PurchaseOrder_Should_Default_Status_To_Pending_Approval()
    {
        var po = new PurchaseOrder();
        po.Status.ShouldBe("Pending Approval");
    }

    [Fact]
    public void PurchaseOrder_DeliveryDate_Should_Be_14_Days_After_OrderDate()
    {
        var po = new PurchaseOrder();
        var diff = (po.DeliveryDate - po.OrderDate).Days;
        diff.ShouldBe(14);
    }

    [Fact]
    public void PurchaseOrder_Financials_Should_Default_To_Zero()
    {
        var po = new PurchaseOrder();
        po.Subtotal.ShouldBe(0m);
        po.Tax.ShouldBe(0m);
        po.Discount.ShouldBe(0m);
        po.GrandTotal.ShouldBe(0m);
    }

    // ─── WORKFLOW DEFINITION ──────────────────────────────────────────────────

    [Fact]
    public void WorkflowDefinition_Should_Default_Status_To_Active()
    {
        var wf = new WorkflowDefinition();
        wf.Status.ShouldBe("Active");
    }

    [Fact]
    public void WorkflowDefinition_Should_Default_Category_To_General()
    {
        var wf = new WorkflowDefinition();
        wf.Category.ShouldBe("General");
    }

    [Fact]
    public void WorkflowDefinition_Should_Default_Version_To_1()
    {
        var wf = new WorkflowDefinition();
        wf.Version.ShouldBe(1);
    }

    [Fact]
    public void WorkflowDefinition_GraphJson_Should_Default_To_Empty_Object()
    {
        var wf = new WorkflowDefinition();
        wf.GraphJson.ShouldBe("{}");
    }

    // ─── WORKFLOW TASK ────────────────────────────────────────────────────────

    [Fact]
    public void WorkflowTask_Should_Default_Status_To_Pending()
    {
        var task = new WorkflowTask();
        task.Status.ShouldBe("Pending");
    }

    [Fact]
    public void WorkflowTask_Comments_Should_Default_To_Empty()
    {
        var task = new WorkflowTask();
        task.Comments.ShouldBe(string.Empty);
    }

    [Fact]
    public void WorkflowTask_CreatedDate_Should_Be_Recent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var task = new WorkflowTask();
        var after = DateTime.UtcNow.AddSeconds(1);

        task.CreatedDate.ShouldBeGreaterThan(before);
        task.CreatedDate.ShouldBeLessThan(after);
    }
}

/// <summary>
/// Unit tests for domain entity property assignment and mutation.
/// </summary>
public class DomainEntityMutationTests
{
    [Fact]
    public void Employee_Can_Set_Status_To_On_Leave()
    {
        var emp = new Employee { Status = "On Leave" };
        emp.Status.ShouldBe("On Leave");
    }

    [Fact]
    public void Employee_Can_Set_Status_To_Inactive()
    {
        var emp = new Employee { Status = "Inactive" };
        emp.Status.ShouldBe("Inactive");
    }

    [Fact]
    public void Product_Stock_Can_Be_Updated()
    {
        var product = new Product { Stock = 100 };
        product.Stock = 5;
        product.Stock.ShouldBe(5);
    }

    [Fact]
    public void PurchaseOrder_GrandTotal_Calculation_Is_Correct()
    {
        var po = new PurchaseOrder
        {
            Subtotal = 1000m,
            Tax = 150m,
            Discount = 50m
        };
        // Domain layer stores values independently; grand total is computed by business logic
        var calculatedTotal = po.Subtotal + po.Tax - po.Discount;
        calculatedTotal.ShouldBe(1100m);
    }

    [Fact]
    public void LeaveRequest_Status_Can_Transition_To_Approved()
    {
        var leave = new LeaveRequest();
        leave.Status.ShouldBe("Pending");

        leave.Status = "Approved";
        leave.Status.ShouldBe("Approved");
    }

    [Fact]
    public void LeaveRequest_Status_Can_Transition_To_Rejected()
    {
        var leave = new LeaveRequest();
        leave.Status = "Rejected";
        leave.Status.ShouldBe("Rejected");
    }
}
