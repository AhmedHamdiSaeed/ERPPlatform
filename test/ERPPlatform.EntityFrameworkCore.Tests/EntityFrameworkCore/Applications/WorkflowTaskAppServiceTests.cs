using System;
using System.Threading.Tasks;
using ERPPlatform.Modules.Workflow.Application;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications;

/// <summary>
/// Integration tests for WorkflowTaskAppService covering task lifecycle:
/// Pending → Approved and Pending → Rejected state transitions.
/// </summary>
public class WorkflowTaskAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
{
    private readonly IWorkflowTaskAppService _workflowTaskService;

    public WorkflowTaskAppServiceTests()
    {
        _workflowTaskService = GetRequiredService<IWorkflowTaskAppService>();
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_Should_Persist_Task_With_Pending_Status()
    {
        var taskDto = new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-001",
            WorkflowName = "Leave Approval Workflow",
            RequestedBy = "Alice Johnson",
            Details = "Requesting 5 days annual leave for vacation",
            Status = "Pending"
        };

        var result = await _workflowTaskService.CreateAsync(taskDto);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.TaskNumber.ShouldBe("TSK-TEST-001");
        result.WorkflowName.ShouldBe("Leave Approval Workflow");
        result.Status.ShouldBe("Pending");
    }

    // ─── APPROVE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_Should_Set_Status_To_Approved_And_Persist_Comments()
    {
        var task = await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-010",
            WorkflowName = "Purchase Order Approval",
            RequestedBy = "Bob Williams",
            Details = "PO #PO-2026-441 for $12,500 machinery parts",
            Status = "Pending"
        });

        await _workflowTaskService.ApproveAsync(task.Id, "Approved — budget available for Q3.");

        var updated = await _workflowTaskService.GetAsync(task.Id);
        updated.Status.ShouldBe("Approved");
        updated.Comments.ShouldBe("Approved — budget available for Q3.");
    }

    [Fact]
    public async Task ApproveAsync_With_Empty_Comments_Should_Still_Set_Approved()
    {
        var task = await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-011",
            WorkflowName = "Expense Claim Approval",
            RequestedBy = "Carol Doe",
            Details = "Travel expense claim for $850",
            Status = "Pending"
        });

        await _workflowTaskService.ApproveAsync(task.Id, string.Empty);

        var updated = await _workflowTaskService.GetAsync(task.Id);
        updated.Status.ShouldBe("Approved");
    }

    // ─── REJECT ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectAsync_Should_Set_Status_To_Rejected_And_Persist_Comments()
    {
        var task = await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-020",
            WorkflowName = "Overtime Pay Approval",
            RequestedBy = "David Chen",
            Details = "Overtime claim for 40 hours at time-and-a-half",
            Status = "Pending"
        });

        await _workflowTaskService.RejectAsync(task.Id, "Rejected — overtime not pre-authorized by department head.");

        var updated = await _workflowTaskService.GetAsync(task.Id);
        updated.Status.ShouldBe("Rejected");
        updated.Comments.ShouldBe("Rejected — overtime not pre-authorized by department head.");
    }

    // ─── LIST & DELETE ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetListAsync_Should_Return_Multiple_Tasks()
    {
        await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-030",
            WorkflowName = "Asset Disposal Approval",
            RequestedBy = "Eva Martinez",
            Details = "Dispose of 10 old workstations",
            Status = "Pending"
        });
        await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-031",
            WorkflowName = "Software License Renewal",
            RequestedBy = "Frank Lee",
            Details = "Annual license renewal for CAD suite",
            Status = "Pending"
        });

        var list = await _workflowTaskService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Task_From_Repository()
    {
        var task = await _workflowTaskService.CreateAsync(new WorkflowTaskDto
        {
            TaskNumber = "TSK-TEST-040",
            WorkflowName = "Temporary Hire Approval",
            RequestedBy = "Grace Kim",
            Details = "Request to hire 2 temporary contractors for Q4 project",
            Status = "Pending"
        });

        await _workflowTaskService.DeleteAsync(task.Id);

        await Should.ThrowAsync<Exception>(async () =>
            await _workflowTaskService.GetAsync(task.Id));
    }
}
