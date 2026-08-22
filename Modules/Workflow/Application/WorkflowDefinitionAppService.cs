using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Workflow.Application
{
    public class WorkflowDefinitionDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Status { get; set; } = "Active";
        public string GraphJson { get; set; } = "{}";
        public int Version { get; set; } = 1;
    }

    public interface IWorkflowDefinitionAppService : ICrudAppService<WorkflowDefinitionDto, Guid, PagedAndSortedResultRequestDto, WorkflowDefinitionDto>
    {
    }

    public class WorkflowDefinitionAppService : CrudAppService<WorkflowDefinition, WorkflowDefinitionDto, Guid, PagedAndSortedResultRequestDto, WorkflowDefinitionDto>, IWorkflowDefinitionAppService
    {
        public WorkflowDefinitionAppService(IRepository<WorkflowDefinition, Guid> repository) : base(repository)
        {
        }

        protected override Task<WorkflowDefinition> MapToEntityAsync(WorkflowDefinitionDto createInput)
        {
            return Task.FromResult(new WorkflowDefinition
            {
                Code = createInput.Code,
                Name = createInput.Name,
                Description = createInput.Description,
                Category = string.IsNullOrWhiteSpace(createInput.Category) ? "General" : createInput.Category,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Active" : createInput.Status,
                GraphJson = string.IsNullOrWhiteSpace(createInput.GraphJson) ? "{}" : createInput.GraphJson,
                Version = createInput.Version <= 0 ? 1 : createInput.Version
            });
        }

        protected override Task MapToEntityAsync(WorkflowDefinitionDto updateInput, WorkflowDefinition entity)
        {
            entity.Code = updateInput.Code;
            entity.Name = updateInput.Name;
            entity.Description = updateInput.Description;
            entity.Category = updateInput.Category;
            entity.Status = updateInput.Status;
            entity.GraphJson = string.IsNullOrWhiteSpace(updateInput.GraphJson) ? entity.GraphJson : updateInput.GraphJson;
            if (updateInput.Version > 0)
            {
                entity.Version = updateInput.Version;
            }
            return Task.CompletedTask;
        }

        protected override Task<WorkflowDefinitionDto> MapToGetOutputDtoAsync(WorkflowDefinition entity)
        {
            return Task.FromResult(new WorkflowDefinitionDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                Category = entity.Category,
                Status = entity.Status,
                GraphJson = entity.GraphJson,
                Version = entity.Version
            });
        }
    }
}
