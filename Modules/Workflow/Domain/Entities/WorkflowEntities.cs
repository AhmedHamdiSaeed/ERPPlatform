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
        public bool IsActiveVersion { get; set; } = true;
        public Guid? ParentDefinitionId { get; set; }
        public string VersionNotes { get; set; } = string.Empty;
        public string TriggerType { get; set; } = "Manual"; // Manual, Cron, EntityCdc, Webhook
        public string ExecutionMode { get; set; } = "Sequential"; // Sequential, Parallel, EventDriven
        public int SlaHours { get; set; } = 24;
        public string WebhookSecret { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string CdcEntityName { get; set; } = string.Empty; // e.g. "PurchaseOrder", "Invoice"
        public string CdcEvent { get; set; } = string.Empty; // e.g. "Created", "StatusChanged"
    }

    public class WorkflowTask : FullAuditedAggregateRoot<Guid>
    {
        public string TaskNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByAvatar { get; set; } = string.Empty;
        public string AssignedToRole { get; set; } = string.Empty;
        public string AssignedToUserId { get; set; } = string.Empty;
        public string OriginalAssigneeId { get; set; } = string.Empty;
        public string DelegatedFromUserId { get; set; } = string.Empty;
        public bool IsEscalated { get; set; } = false;
        public string EscalatedToUserId { get; set; } = string.Empty;
        public DateTime? EscalatedAt { get; set; }
        public string ActionToken { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public int StepOrder { get; set; } = 1;
        public string CurrentNodeId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, ChangesRequested
        public string Comments { get; set; } = string.Empty;
        public string SignatureBase64 { get; set; } = string.Empty;
        public string SignedBy { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
    }
}
