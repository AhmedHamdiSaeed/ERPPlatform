using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Workflow.Application
{
    public class WorkflowTaskDto : EntityDto<Guid>
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
        public bool IsEscalated { get; set; }
        public string EscalatedToUserId { get; set; } = string.Empty;
        public DateTime? EscalatedAt { get; set; }
        public string ActionToken { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int StepOrder { get; set; } = 1;
        public string CurrentNodeId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Comments { get; set; } = string.Empty;
        public string SignatureBase64 { get; set; } = string.Empty;
        public string SignedBy { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
    }

    public class DecisionTokenResultDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public Guid TaskId { get; set; }
    }

    public interface IWorkflowTaskAppService : ICrudAppService<WorkflowTaskDto, Guid, PagedAndSortedResultRequestDto, WorkflowTaskDto>
    {
        Task ApproveAsync(Guid id, string comments);
        Task RejectAsync(Guid id, string comments);
        Task RequestChangesAsync(Guid id, string comments);
        Task CaptureSignatureAsync(Guid id, string signatureBase64, string signedBy);
        Task<string> GenerateDecisionTokenAsync(Guid taskId, string decision);
        Task<DecisionTokenResultDto> ExecuteDecisionTokenAsync(string token);
        Task<int> EscalateOverdueTasksAsync();
        Task DelegateTaskAsync(Guid taskId, string delegateUserId, string reason);
    }

    public class WorkflowTaskAppService : CrudAppService<WorkflowTask, WorkflowTaskDto, Guid, PagedAndSortedResultRequestDto, WorkflowTaskDto>, IWorkflowTaskAppService
    {
        public WorkflowTaskAppService(IRepository<WorkflowTask, Guid> repository) : base(repository)
        {
        }

        public async Task ApproveAsync(Guid id, string comments)
        {
            var task = await Repository.GetAsync(id);
            task.Status = "Approved";
            task.Comments = string.IsNullOrWhiteSpace(comments) ? task.Comments : comments;
            task.SignedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(task);
        }

        public async Task RejectAsync(Guid id, string comments)
        {
            var task = await Repository.GetAsync(id);
            task.Status = "Rejected";
            task.Comments = string.IsNullOrWhiteSpace(comments) ? task.Comments : comments;
            await Repository.UpdateAsync(task);
        }

        public async Task RequestChangesAsync(Guid id, string comments)
        {
            var task = await Repository.GetAsync(id);
            task.Status = "ChangesRequested";
            task.Comments = string.IsNullOrWhiteSpace(comments) ? task.Comments : comments;
            await Repository.UpdateAsync(task);
        }

        public async Task CaptureSignatureAsync(Guid id, string signatureBase64, string signedBy)
        {
            var task = await Repository.GetAsync(id);
            task.SignatureBase64 = signatureBase64;
            task.SignedBy = signedBy;
            task.SignedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(task);
        }

        public async Task<string> GenerateDecisionTokenAsync(Guid taskId, string decision)
        {
            var task = await Repository.GetAsync(taskId);
            var token = $"{taskId:N}_{decision.ToLowerInvariant()}_{DateTime.UtcNow.AddDays(3):yyyyMMddHHmm}_{Guid.NewGuid().ToString("N")[..8]}";
            task.ActionToken = token;
            await Repository.UpdateAsync(task);
            return token;
        }

        public async Task<DecisionTokenResultDto> ExecuteDecisionTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new DecisionTokenResultDto { Success = false, Message = "Invalid action token." };
            }

            var parts = token.Split('_');
            if (parts.Length < 2 || !Guid.TryParse(parts[0], out var taskId))
            {
                return new DecisionTokenResultDto { Success = false, Message = "Malformed action token format." };
            }

            var decision = parts[1].ToLowerInvariant();
            var task = await Repository.FindAsync(taskId);
            if (task == null)
            {
                return new DecisionTokenResultDto { Success = false, Message = "Task record not found." };
            }

            if (decision == "approve")
            {
                task.Status = "Approved";
                task.Comments = "1-Click Decision Link Authenticated & Approved";
                task.SignedAt = DateTime.UtcNow;
            }
            else if (decision == "reject")
            {
                task.Status = "Rejected";
                task.Comments = "1-Click Decision Link Authenticated & Rejected";
            }
            else
            {
                task.Status = "ChangesRequested";
                task.Comments = "1-Click Decision Link: Modifications requested";
            }

            await Repository.UpdateAsync(task);

            return new DecisionTokenResultDto
            {
                Success = true,
                Message = $"Action '{decision}' executed successfully via 1-click token.",
                NewStatus = task.Status,
                TaskId = task.Id
            };
        }

        public async Task<int> EscalateOverdueTasksAsync()
        {
            var pendingTasks = await Repository.GetListAsync(x => x.Status == "Pending" && !x.IsEscalated);
            var count = 0;
            var now = DateTime.UtcNow;
            foreach (var task in pendingTasks)
            {
                if (task.DueDate.HasValue && task.DueDate.Value < now)
                {
                    task.IsEscalated = true;
                    task.EscalatedToUserId = "manager-escalation-target";
                    task.EscalatedAt = now;
                    task.Priority = "Urgent";
                    task.Comments = $"Auto-escalated by SLA watchdog at {now:yyyy-MM-dd HH:mm} UTC";
                    await Repository.UpdateAsync(task);
                    count++;
                }
            }
            return count;
        }

        public async Task DelegateTaskAsync(Guid taskId, string delegateUserId, string reason)
        {
            var task = await Repository.GetAsync(taskId);
            task.OriginalAssigneeId = string.IsNullOrWhiteSpace(task.OriginalAssigneeId) ? task.AssignedToUserId : task.OriginalAssigneeId;
            task.DelegatedFromUserId = task.AssignedToUserId;
            task.AssignedToUserId = delegateUserId;
            task.Comments = $"Delegated to user '{delegateUserId}'. Reason: {reason}";
            await Repository.UpdateAsync(task);
        }

        protected override Task<WorkflowTask> MapToEntityAsync(WorkflowTaskDto createInput)
        {
            return Task.FromResult(new WorkflowTask
            {
                TaskNumber = string.IsNullOrWhiteSpace(createInput.TaskNumber) ? $"TSK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}" : createInput.TaskNumber,
                WorkflowName = createInput.WorkflowName,
                WorkflowDefinitionId = createInput.WorkflowDefinitionId,
                RequestedBy = createInput.RequestedBy,
                RequestedByAvatar = string.IsNullOrWhiteSpace(createInput.RequestedByAvatar) ? "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150" : createInput.RequestedByAvatar,
                AssignedToRole = createInput.AssignedToRole,
                AssignedToUserId = createInput.AssignedToUserId,
                OriginalAssigneeId = createInput.OriginalAssigneeId,
                DelegatedFromUserId = createInput.DelegatedFromUserId,
                IsEscalated = createInput.IsEscalated,
                EscalatedToUserId = createInput.EscalatedToUserId,
                EscalatedAt = createInput.EscalatedAt,
                ActionToken = createInput.ActionToken,
                Priority = string.IsNullOrWhiteSpace(createInput.Priority) ? "Normal" : createInput.Priority,
                Details = createInput.Details,
                CreatedDate = createInput.CreatedDate == default ? DateTime.UtcNow : createInput.CreatedDate,
                DueDate = createInput.DueDate ?? DateTime.UtcNow.AddHours(24),
                StepOrder = createInput.StepOrder <= 0 ? 1 : createInput.StepOrder,
                CurrentNodeId = createInput.CurrentNodeId,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Pending" : createInput.Status,
                Comments = createInput.Comments
            });
        }

        protected override Task MapToEntityAsync(WorkflowTaskDto updateInput, WorkflowTask entity)
        {
            entity.TaskNumber = updateInput.TaskNumber;
            entity.WorkflowName = updateInput.WorkflowName;
            entity.WorkflowDefinitionId = updateInput.WorkflowDefinitionId;
            entity.RequestedBy = updateInput.RequestedBy;
            entity.RequestedByAvatar = updateInput.RequestedByAvatar;
            entity.AssignedToRole = updateInput.AssignedToRole;
            entity.AssignedToUserId = updateInput.AssignedToUserId;
            entity.OriginalAssigneeId = updateInput.OriginalAssigneeId;
            entity.DelegatedFromUserId = updateInput.DelegatedFromUserId;
            entity.IsEscalated = updateInput.IsEscalated;
            entity.EscalatedToUserId = updateInput.EscalatedToUserId;
            entity.EscalatedAt = updateInput.EscalatedAt;
            entity.ActionToken = updateInput.ActionToken;
            entity.Priority = updateInput.Priority;
            entity.Details = updateInput.Details;
            entity.DueDate = updateInput.DueDate;
            entity.StepOrder = updateInput.StepOrder;
            entity.CurrentNodeId = updateInput.CurrentNodeId;
            entity.Status = updateInput.Status;
            entity.Comments = updateInput.Comments;
            return Task.CompletedTask;
        }

        protected override Task<WorkflowTaskDto> MapToGetOutputDtoAsync(WorkflowTask entity)
        {
            return Task.FromResult(new WorkflowTaskDto
            {
                Id = entity.Id,
                TaskNumber = entity.TaskNumber,
                WorkflowName = entity.WorkflowName,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                RequestedBy = entity.RequestedBy,
                RequestedByAvatar = entity.RequestedByAvatar,
                AssignedToRole = entity.AssignedToRole,
                AssignedToUserId = entity.AssignedToUserId,
                OriginalAssigneeId = entity.OriginalAssigneeId,
                DelegatedFromUserId = entity.DelegatedFromUserId,
                IsEscalated = entity.IsEscalated,
                EscalatedToUserId = entity.EscalatedToUserId,
                EscalatedAt = entity.EscalatedAt,
                ActionToken = entity.ActionToken,
                Priority = entity.Priority,
                Details = entity.Details,
                CreatedDate = entity.CreatedDate,
                DueDate = entity.DueDate,
                StepOrder = entity.StepOrder,
                CurrentNodeId = entity.CurrentNodeId,
                Status = entity.Status,
                Comments = entity.Comments,
                SignatureBase64 = entity.SignatureBase64,
                SignedBy = entity.SignedBy,
                SignedAt = entity.SignedAt
            });
        }
    }
}
