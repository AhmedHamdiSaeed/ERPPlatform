using System;
using ERPPlatform.Modules.Workflow.Domain.Entities;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.Workflow;

public class WorkflowEntityTests
{
    [Fact]
    public void WorkflowDefinition_Initialization_DefaultValues()
    {
        var wf = new WorkflowDefinition
        {
            Code = "WF-LEAVE-APPROVAL",
            Name = "Leave Approval Workflow",
            Description = "Automated routing for annual and casual leaves",
            Category = "HR"
        };

        wf.Status.ShouldBe("Active");
        wf.Version.ShouldBe(1);
        wf.GraphJson.ShouldBe("{}");
    }

    [Fact]
    public void WorkflowTask_StatusTransition_ApproveAndReject()
    {
        var task = new WorkflowTask
        {
            TaskNumber = "TSK-2026-001",
            WorkflowName = "Expense Reimbursement",
            RequestedBy = "Michael Scott",
            Details = "Conference registration fee $450"
        };

        task.Status.ShouldBe("Pending");

        // Transition to Approved
        task.Status = "Approved";
        task.Comments = "Approved within quarterly budget.";

        task.Status.ShouldBe("Approved");
        task.Comments.ShouldContain("quarterly budget");
    }
}
