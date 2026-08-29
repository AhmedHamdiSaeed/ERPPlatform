using AutoMapper;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Modules.Workflow.Application
{
    public class WorkflowApplicationAutoMapperProfile : Profile
    {
        public WorkflowApplicationAutoMapperProfile()
        {
            CreateMap<WorkflowDefinition, WorkflowDefinitionDto>().ReverseMap();
            CreateMap<WorkflowTask, WorkflowTaskDto>().ReverseMap();
        }
    }
}