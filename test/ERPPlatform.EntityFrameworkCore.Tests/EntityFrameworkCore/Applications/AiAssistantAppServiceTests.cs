using System;
using System.Threading.Tasks;
using ERPPlatform.Modules.AI.Application;
using Shouldly;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications;

/// <summary>
/// Unit tests for AiAssistantAppService (no DB dependency — service is pure logic).
/// Tests AI response generation and executive summary formatting.
/// </summary>
public class AiAssistantAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
{
    private readonly IAiAssistantAppService _aiService;

    public AiAssistantAppServiceTests()
    {
        _aiService = GetRequiredService<IAiAssistantAppService>();
    }

    // ─── ASK ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AskAsync_Should_Return_Non_Null_Response()
    {
        var response = await _aiService.AskAsync(new AiPromptRequestDto
        {
            Prompt = "What is the current system status?"
        });

        response.ShouldNotBeNull();
        response.Answer.ShouldNotBeNullOrWhiteSpace();
        response.Timestamp.ShouldNotBe(default(DateTime));
    }

    [Fact]
    public async Task AskAsync_Should_Include_AI_Intelligence_Header_In_Answer()
    {
        var response = await _aiService.AskAsync(new AiPromptRequestDto
        {
            Prompt = "Analyze employee productivity"
        });

        response.Answer.ShouldContain("[AI Intelligence]");
    }

    [Fact]
    public async Task AskAsync_Should_Return_Workflow_Json_When_Prompt_Contains_Workflow()
    {
        var response = await _aiService.AskAsync(new AiPromptRequestDto
        {
            Prompt = "Create a workflow for approval"
        });

        response.GeneratedWorkflowJson.ShouldNotBeNullOrEmpty();
        response.GeneratedWorkflowJson.ShouldContain("nodes");
    }

    [Fact]
    public async Task AskAsync_Should_Return_Workflow_Json_When_Prompt_Contains_Leave()
    {
        var response = await _aiService.AskAsync(new AiPromptRequestDto
        {
            Prompt = "Show me leave request status"
        });

        response.GeneratedWorkflowJson.ShouldNotBeNullOrEmpty();
        response.GeneratedWorkflowJson.ShouldContain("Leave Trigger");
    }

    [Fact]
    public async Task AskAsync_Should_Return_Empty_Workflow_Json_For_Non_Workflow_Prompts()
    {
        var response = await _aiService.AskAsync(new AiPromptRequestDto
        {
            Prompt = "What are the current financial KPIs?"
        });

        response.GeneratedWorkflowJson.ShouldBe(string.Empty);
    }

    // ─── EXECUTIVE SUMMARY ───────────────────────────────────────────────────

    [Fact]
    public async Task GetExecutiveSummaryAsync_Should_Return_Non_Empty_Summary()
    {
        var summary = await _aiService.GetExecutiveSummaryAsync();

        summary.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetExecutiveSummaryAsync_Should_Contain_Executive_Header()
    {
        var summary = await _aiService.GetExecutiveSummaryAsync();

        summary.ShouldContain("Executive ERP Summary");
    }

    [Fact]
    public async Task GetExecutiveSummaryAsync_Should_Include_Employee_Count()
    {
        var summary = await _aiService.GetExecutiveSummaryAsync();

        summary.ShouldContain("Total Employees");
    }

    [Fact]
    public async Task GetExecutiveSummaryAsync_Should_Include_Inventory_Value()
    {
        var summary = await _aiService.GetExecutiveSummaryAsync();

        summary.ShouldContain("Total Inventory Stock Value");
    }
}
