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
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByAvatar { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string Comments { get; set; } = string.Empty;
    }

    public interface IWorkflowTaskAppService : ICrudAppService<WorkflowTaskDto, Guid, PagedAndSortedResultRequestDto, WorkflowTaskDto>
    {
        Task ApproveAsync(Guid id, string comments);
        Task RejectAsync(Guid id, string comments);
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
            task.Comments = comments;
            await Repository.UpdateAsync(task);
        }

        public async Task RejectAsync(Guid id, string comments)
        {
            var task = await Repository.GetAsync(id);
            task.Status = "Rejected";
            task.Comments = comments;
            await Repository.UpdateAsync(task);
        }

        protected override Task<WorkflowTask> MapToEntityAsync(WorkflowTaskDto createInput)
        {
            return Task.FromResult(new WorkflowTask
            {
                TaskNumber = createInput.TaskNumber,
                WorkflowName = createInput.WorkflowName,
                RequestedBy = createInput.RequestedBy,
                RequestedByAvatar = createInput.RequestedByAvatar,
                Details = createInput.Details,
                CreatedDate = createInput.CreatedDate == default ? DateTime.UtcNow : createInput.CreatedDate,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Pending" : createInput.Status,
                Comments = createInput.Comments
            });
        }

        protected override Task MapToEntityAsync(WorkflowTaskDto updateInput, WorkflowTask entity)
        {
            entity.TaskNumber = updateInput.TaskNumber;
            entity.WorkflowName = updateInput.WorkflowName;
            entity.RequestedBy = updateInput.RequestedBy;
            entity.RequestedByAvatar = updateInput.RequestedByAvatar;
            entity.Details = updateInput.Details;
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
                RequestedBy = entity.RequestedBy,
                RequestedByAvatar = entity.RequestedByAvatar,
                Details = entity.Details,
                CreatedDate = entity.CreatedDate,
                Status = entity.Status,
                Comments = entity.Comments
            });
        }
    }
}
