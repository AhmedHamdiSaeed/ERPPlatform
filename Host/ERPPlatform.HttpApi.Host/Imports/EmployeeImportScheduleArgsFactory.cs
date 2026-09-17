using System;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace ERPPlatform.Imports;

/// <summary>
/// Rebuilds scheduler arguments for a job so the recovery watchdog and the retry path
/// do not need to remember who started an import. The creator and tenant come from the
/// persisted job row.
/// </summary>
public class EmployeeImportScheduleArgsFactory : IEmployeeImportScheduleArgsFactory, ITransientDependency
{
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;
    private readonly IDataFilter _dataFilter;

    public EmployeeImportScheduleArgsFactory(
        IRepository<EmployeeImportJob, Guid> jobRepository,
        IDataFilter dataFilter)
    {
        _jobRepository = jobRepository;
        _dataFilter = dataFilter;
    }

    public async Task<EmployeeImportScheduleArgs?> CreateAsync(Guid importJobId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
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
}
