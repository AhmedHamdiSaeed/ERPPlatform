using AutoMapper;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Maps the import aggregate to its DTOs. Discovered automatically by
/// <c>AddMaps&lt;ERPPlatformApplicationModule&gt;()</c>.
/// </summary>
public class EmployeeImportAutoMapperProfile : Profile
{
    public EmployeeImportAutoMapperProfile()
    {
        CreateMap<EmployeeImportJob, EmployeeImportJobDto>();
        CreateMap<EmployeeImportJob, EmployeeImportDetailsDto>();
        CreateMap<EmployeeImportChunk, EmployeeImportChunkDto>();
        CreateMap<EmployeeImportError, EmployeeImportErrorDto>();
    }
}
