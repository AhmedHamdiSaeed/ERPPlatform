using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Resumable, chunked employee Excel import.
///
/// Conventional ABP routes (root path <c>app</c>):
///   POST /api/app/employee-import/import-employees      -> start an import
///   GET  /api/app/employee-import                       -> history (paged/filtered)
///   GET  /api/app/employee-import/active                -> jobs still running (used to restore UI after a refresh)
///   GET  /api/app/employee-import/{id}                  -> details incl. chunks
///   GET  /api/app/employee-import/{id}/status           -> lightweight progress poll
///   GET  /api/app/employee-import/{id}/chunks           -> chunk list
///   GET  /api/app/employee-import/{id}/errors           -> rejected rows
///   POST /api/app/employee-import/{id}/retry            -> re-queue only incomplete chunks
///   POST /api/app/employee-import/{id}/cancel           -> persist cancellation
/// </summary>
public interface IEmployeeImportAppService : IApplicationService
{
    /// <summary>
    /// Validates the spreadsheet, persists it, creates the job + chunk rows and
    /// enqueues the background work. Returns as soon as the job is queued.
    /// </summary>
    Task<EmployeeImportStartResultDto> ImportEmployeesAsync(EmployeeImportInput input);

    Task<PagedResultDto<EmployeeImportJobDto>> GetListAsync(EmployeeImportJobListInput input);

    Task<ListResultDto<EmployeeImportJobDto>> GetActiveAsync();

    Task<EmployeeImportDetailsDto> GetAsync(Guid id);

    Task<EmployeeImportJobDto> GetStatusAsync(Guid id);

    Task<ListResultDto<EmployeeImportChunkDto>> GetChunksAsync(Guid id);

    Task<PagedResultDto<EmployeeImportErrorDto>> GetErrorsAsync(Guid id, EmployeeImportErrorListInput input);

    /// <summary>
    /// Re-queues only the chunks that are not <c>Completed</c>. Chunks already done
    /// are left untouched, so a retry never restarts the file from row 1.
    /// </summary>
    Task<EmployeeImportJobDto> RetryAsync(Guid id);

    Task<EmployeeImportJobDto> CancelAsync(Guid id);

    /// <summary>Downloads a ready-to-fill sample workbook.</summary>
    Task<IRemoteStreamContent> GetTemplateAsync();
}
