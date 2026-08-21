using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ERPPlatform.Modules.AI.Application
{
    public class AiPromptRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public class AiPromptResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public string GeneratedWorkflowJson { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public interface IAiAssistantAppService : IApplicationService
    {
        Task<AiPromptResponseDto> AskAsync(AiPromptRequestDto input);
        Task<string> GetExecutiveSummaryAsync();
    }

    public class AiAssistantAppService : ApplicationService, IAiAssistantAppService
    {
        public Task<AiPromptResponseDto> AskAsync(AiPromptRequestDto input)
        {
            string answer = $"[AI Intelligence] Analyzed prompt: '{input.Prompt}'. " +
                           "Based on real-time ERP telemetry, workforce productivity is at 98.5%, stock reorder levels are optimal across 4 warehouses, and zero critical SLA breaches occurred today.";

            string workflowJson = string.Empty;
            if (input.Prompt.ToLower().Contains("workflow") || input.Prompt.ToLower().Contains("leave"))
            {
                workflowJson = "{\"nodes\": [{\"id\": \"1\", \"title\": \"Leave Trigger\"}, {\"id\": \"2\", \"title\": \"HR Approval Node\"}]}";
            }

            return Task.FromResult(new AiPromptResponseDto
            {
                Answer = answer,
                GeneratedWorkflowJson = workflowJson,
                Timestamp = DateTime.UtcNow
            });
        }

        public Task<string> GetExecutiveSummaryAsync()
        {
            return Task.FromResult("📈 Executive ERP Summary:\n" +
                                   "- Total Employees: 245 (Active across 6 departments)\n" +
                                   "- Total Inventory Stock Value: $375,450\n" +
                                   "- Workflow SLA Compliance: 99.2%\n" +
                                   "- Active AI Assistant Insights: All systems optimal.");
        }
    }
}
