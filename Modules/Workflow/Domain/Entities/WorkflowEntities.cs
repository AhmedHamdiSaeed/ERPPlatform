using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Modules.Workflow.Domain.Entities
{
    public class WorkflowDefinition : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Status { get; set; } = "Active"; // Active, Draft, Archived
        public string GraphJson { get; set; } = "{}"; // JSON representation of visual node graph
        public int Version { get; set; } = 1;
    }

    public class WorkflowTask : FullAuditedAggregateRoot<Guid>
    {
        public string TaskNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByAvatar { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string Comments { get; set; } = string.Empty;
    }
}
