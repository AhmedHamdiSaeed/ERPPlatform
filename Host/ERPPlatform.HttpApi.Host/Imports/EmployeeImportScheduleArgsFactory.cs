using System;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Imports;

/// <summary>
/// Rebuilds scheduler arguments for a job so the recovery watchdog and the retry path
/// do not need to remember who started an import. The creator and tenant come from the
/// persisted job row.
/// </summary>
public class EmployeeImportScheduleArgsFactory : IEmployeeImportScheduleArgsFactory, ITransientDependency
{
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;

    public EmployeeImportScheduleArgsFactory(IRepository<EmployeeImportJob, Guid> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<EmployeeImportScheduleArgs?> CreateAsync(Guid importJobId)
    {
        var job = await _jobRepository.FindAsync(importJobId);
        if (job == null)
        {
            return null;
        }

        return new EmployeeImportScheduleArgs
        {
            ImportJobId = importJobId,
            TenantId = job.TenantId,
            UserId = job.CreatorId?.ToString() ?? string.Empty,
            UserName = job.CreatedByUserName
        };
    }
}
