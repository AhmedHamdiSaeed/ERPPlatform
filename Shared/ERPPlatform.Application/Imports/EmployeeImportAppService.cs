using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using ERPPlatform.Domain.Imports;
using ERPPlatform.Imports;
using ERPPlatform.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Public, authorised surface for the resumable employee Excel import. The heavy
/// lifting (chunk processing, scheduling) lives in <see cref="EmployeeImportChunkProcessor"/>
/// and <see cref="EmployeeImportOrchestrator"/>.
///
/// Routes (ABP conventional controller, root <c>app</c>):
///   POST /api/app/employee-import/import-employees
///   GET  /api/app/employee-import
///   GET  /api/app/employee-import/active
///   GET  /api/app/employee-import/{id}
///   GET  /api/app/employee-import/{id}/status
///   GET  /api/app/employee-import/{id}/chunks
///   GET  /api/app/employee-import/{id}/errors
///   POST /api/app/employee-import/{id}/retry
///   POST /api/app/employee-import/{id}/cancel
///   GET  /api/app/employee-import/template
/// </summary>
[Authorize(ERPPlatformPermissions.EmployeeImport.Default)]
public class EmployeeImportAppService : ERPPlatformAppService, IEmployeeImportAppService
{
    private readonly EmployeeImportOrchestrator _orchestrator;
    private readonly EmployeeImportProgressService _progress;
    private readonly IEmployeeImportExcelReader _excelReader;
    private readonly IEmployeeImportJobScheduler _scheduler;
    private readonly IBlobContainer<EmployeeImportContainer> _blobContainer;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<EmployeeImportJob, Guid> _jobRepository;
    private readonly IRepository<EmployeeImportChunk, Guid> _chunkRepository;
    private readonly IRepository<EmployeeImportError, Guid> _errorRepository;
    private readonly EmployeeImportOptions _options;
    private readonly IClock _clock;

    public EmployeeImportAppService(
        EmployeeImportOrchestrator orchestrator,
        EmployeeImportProgressService progress,
        IEmployeeImportExcelReader excelReader,
        IEmployeeImportJobScheduler scheduler,
        IBlobContainer<EmployeeImportContainer> blobContainer,
        ICurrentUser currentUser,
        IRepository<EmployeeImportJob, Guid> jobRepository,
        IRepository<EmployeeImportChunk, Guid> chunkRepository,
        IRepository<EmployeeImportError, Guid> errorRepository,
        IOptions<EmployeeImportOptions> options,
        IClock clock)
    {
        _orchestrator = orchestrator;
        _progress = progress;
        _excelReader = excelReader;
        _scheduler = scheduler;
        _blobContainer = blobContainer;
        _currentUser = currentUser;
        _jobRepository = jobRepository;
        _chunkRepository = chunkRepository;
        _errorRepository = errorRepository;
        _options = options.Value;
        _clock = clock;
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.Create)]
    public virtual async Task<EmployeeImportStartResultDto> ImportEmployeesAsync(EmployeeImportInput input)
    {
        var streamContent = input.File;
        if (streamContent == null)
        {
            throw new UserFriendlyException(L["InvalidExcelFile"]);
        }

        var fileName = Path.GetFileName(streamContent.FileName ?? "employees.xlsx");
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !EmployeeImportConsts.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessException(EmployeeImportErrorCodes.InvalidFileType,
                $"Unsupported file type '{ext}'. Only .xlsx and .xls are allowed.");
        }

        using var memory = new MemoryStream();
        await streamContent.GetStream().CopyToAsync(memory);
        var bytes = memory.ToArray();

        if (bytes.Length == 0)
        {
            throw new BusinessException(EmployeeImportErrorCodes.EmptyFile, "The uploaded file is empty.");
        }

        if (bytes.Length > _options.MaxFileSizeBytes)
        {
            throw new BusinessException(EmployeeImportErrorCodes.FileTooLarge,
                $"The file is {bytes.Length / (1024 * 1024)} MB but the maximum allowed is {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var fileHash = ComputeHash(bytes);

        // Duplicate submission guard: same user, same file, still running.
        var duplicate = await _jobRepository.FirstOrDefaultAsync(j =>
            j.CreatorId == _currentUser.Id &&
            j.FileHash == fileHash &&
            (j.Status == EmployeeImportStatus.Queued || j.Status == EmployeeImportStatus.Processing));
        if (duplicate != null)
        {
            throw new BusinessException(EmployeeImportErrorCodes.DuplicateSubmission,
                "This exact file is already being imported under an active job.");
        }

        // Up-front structural validation — fails fast with useful messages.
        EmployeeImportScanResult scan;
        try
        {
            scan = _excelReader.Scan(new MemoryStream(bytes), _options.MaxTotalRows);
        }
        catch (Exception ex)
        {
            throw new BusinessException(EmployeeImportErrorCodes.InvalidWorkbook,
                $"Could not read the spreadsheet: {ex.Message}");
        }

        if (!scan.IsValid)
        {
            var problems = scan.Errors
                .Select(e => e.ErrorMessage)
                .Concat(scan.DuplicateErrors.Select(e => e.ErrorMessage))
                .ToList();

            throw new BusinessException(EmployeeImportErrorCodes.MissingColumns,
                problems.Count == 0 ? "The spreadsheet could not be validated." : problems[0])
            {
                Details = string.Join("\n", problems)
            };
        }

        var storageKey = $"jobs/{_clock.Now:yyyyMMdd}/{Guid.NewGuid():N}/{fileName}";
        await _blobContainer.SaveAsync(storageKey, bytes, overrideExisting: true);

        var scheduleArgs = new EmployeeImportScheduleArgs
        {
            ImportJobId = Guid.Empty,
            TenantId = _currentUser.TenantId,
            UserId = _currentUser.Id?.ToString() ?? string.Empty,
            UserName = _currentUser.UserName ?? _currentUser.Email ?? "system"
        };

        var job = await _orchestrator.StartAsync(fileName, bytes.Length, fileHash, storageKey, scan, scheduleArgs);

        // Queue the very first chunk. The API returns immediately; the rest runs in
        // the background and the client tracks progress over SignalR + polling.
        await _orchestrator.QueueFirstChunkAsync(scheduleArgs);

        return new EmployeeImportStartResultDto
        {
            ImportJobId = job.Id,
            Status = job.Status.ToString(),
            FileName = job.FileName,
            FileSize = job.FileSize,
            TotalRows = job.TotalRows,
            TotalChunks = job.TotalChunks,
            ChunkSize = job.ChunkSize
        };
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.View)]
    public virtual async Task<PagedResultDto<EmployeeImportJobDto>> GetListAsync(EmployeeImportJobListInput input)
    {
        var query = await _jobRepository.GetQueryableAsync();

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search), j =>
                j.FileName.Contains(input.Search!))
            .WhereIf(!string.IsNullOrWhiteSpace(input.FileName), j =>
                j.FileName.Contains(input.FileName!))
            .WhereIf(input.Status.HasValue, j => j.Status == input.Status!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.StartedBy), j =>
                j.CreatedByUserName.Contains(input.StartedBy!))
            .WhereIf(input.FromDate.HasValue, j => j.CreationTime >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, j => j.CreationTime <= input.ToDate!.Value);

        var total = await AsyncExecuter.CountAsync(query);

        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        var items = await AsyncExecuter.ToListAsync(query);
        return new PagedResultDto<EmployeeImportJobDto>(total, items.Select(MapJob).ToList());
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.View)]
    public virtual async Task<ListResultDto<EmployeeImportJobDto>> GetActiveAsync()
    {
        var query = await _jobRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(query.Where(j =>
            j.Status == EmployeeImportStatus.Queued || j.Status == EmployeeImportStatus.Processing));

        // Most recently touched first so a freshly started import appears immediately.
        items = items.OrderByDescending(j => j.LastModificationTime ?? j.CreationTime).ToList();
        return new ListResultDto<EmployeeImportJobDto>(items.Select(MapJob).ToList());
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.View)]
    public virtual async Task<EmployeeImportDetailsDto> GetAsync(Guid id)
    {
        var job = await GetJobOrThrowAsync(id);
        var details = ObjectMapper.Map<EmployeeImportJob, EmployeeImportDetailsDto>(job);

        var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == id);
        details.Chunks = chunks.OrderBy(c => c.ChunkNumber)
            .Select(c => ObjectMapper.Map<EmployeeImportChunk, EmployeeImportChunkDto>(c))
            .ToList();

        return details;
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.View)]
    public virtual async Task<EmployeeImportJobDto> GetStatusAsync(Guid id)
    {
        var job = await GetJobOrThrowAsync(id);
        return MapJob(job);
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.View)]
    public virtual async Task<ListResultDto<EmployeeImportChunkDto>> GetChunksAsync(Guid id)
    {
        await GetJobOrThrowAsync(id);
        var chunks = await _chunkRepository.GetListAsync(c => c.ImportJobId == id);
        return new ListResultDto<EmployeeImportChunkDto>(
            chunks.OrderBy(c => c.ChunkNumber)
                .Select(c => ObjectMapper.Map<EmployeeImportChunk, EmployeeImportChunkDto>(c))
                .ToList());
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.ViewErrors)]
    public virtual async Task<PagedResultDto<EmployeeImportErrorDto>> GetErrorsAsync(Guid id, EmployeeImportErrorListInput input)
    {
        await GetJobOrThrowAsync(id);

        var query = (await _errorRepository.GetQueryableAsync())
            .Where(e => e.ImportJobId == id)
            .WhereIf(input.RowNumber.HasValue, e => e.RowNumber == input.RowNumber!.Value);

        var total = await AsyncExecuter.CountAsync(query);

        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        var items = await AsyncExecuter.ToListAsync(query);
        return new PagedResultDto<EmployeeImportErrorDto>(total,
            items.Select(e => ObjectMapper.Map<EmployeeImportError, EmployeeImportErrorDto>(e)).ToList());
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.Retry)]
    public virtual async Task<EmployeeImportJobDto> RetryAsync(Guid id)
    {
        var job = await GetJobOrThrowAsync(id);

        if (job.Status is EmployeeImportStatus.Completed or EmployeeImportStatus.CompletedWithErrors)
        {
            throw new BusinessException(EmployeeImportErrorCodes.NotRetryable,
                "Only a job that failed or was cancelled can be retried.");
        }

        var args = new EmployeeImportScheduleArgs
        {
            ImportJobId = id,
            TenantId = _currentUser.TenantId,
            UserId = _currentUser.Id?.ToString() ?? string.Empty,
            UserName = _currentUser.UserName ?? _currentUser.Email ?? "system"
        };

        await _orchestrator.ResumeAsync(id, args);

        job = await GetJobOrThrowAsync(id);
        return MapJob(job);
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.Cancel)]
    public virtual async Task<EmployeeImportJobDto> CancelAsync(Guid id)
    {
        var job = await GetJobOrThrowAsync(id);

        if (job.IsFinished)
        {
            throw new BusinessException(EmployeeImportErrorCodes.NotCancellable,
                "A finished import cannot be cancelled.");
        }

        await _orchestrator.CancelAsync(id);

        job = await GetJobOrThrowAsync(id);
        return MapJob(job);
    }

    [Authorize(ERPPlatformPermissions.EmployeeImport.Create)]
    public virtual async Task<IRemoteStreamContent> GetTemplateAsync()
    {
        var bytes = _excelReader.BuildTemplate();
        return new RemoteStreamContent(
            new MemoryStream(bytes),
            "EmployeeImportTemplate.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    // ── Helpers ─────────────────────────────────────────────────

    private async Task<EmployeeImportJob> GetJobOrThrowAsync(Guid id)
    {
        var job = await _jobRepository.FindAsync(id);
        if (job == null)
        {
            throw new BusinessException(EmployeeImportErrorCodes.JobNotFound, "Import job not found.");
        }

        return job;
    }

    private EmployeeImportJobDto MapJob(EmployeeImportJob job) => new()
    {
        Id = job.Id,
        FileName = job.FileName,
        FileSize = job.FileSize,
        Status = job.Status,
        TotalRows = job.TotalRows,
        ProcessedRows = job.ProcessedRows,
        SuccessfulRows = job.SuccessfulRows,
        FailedRows = job.FailedRows,
        ChunkSize = job.ChunkSize,
        TotalChunks = job.TotalChunks,
        CompletedChunks = job.CompletedChunks,
        FailedChunks = job.FailedChunks,
        CurrentChunk = job.CurrentChunk,
        ProgressPercentage = job.ProgressPercentage,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
        CancelledAt = job.CancelledAt,
        CreationTime = job.CreationTime,
        CreatedByUserName = job.CreatedByUserName,
        RetryCount = job.RetryCount,
        LastError = job.LastError,
        CanRetry = job.Status is EmployeeImportStatus.Failed or EmployeeImportStatus.Cancelled,
        CanCancel = job.IsActive
    };

    private static string ComputeHash(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }

    private static IQueryable<EmployeeImportJob> ApplySorting(IQueryable<EmployeeImportJob> query, EmployeeImportJobListInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.Sorting) && input.Sorting.Contains("desc", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(j => j.CreationTime);
        }

        return query.OrderBy(j => j.CreationTime);
    }

    /// <summary>
    /// Errors are most useful in spreadsheet order — CreationTime is near-identical for every
    /// row in a chunk, so sorting by it would look random to the user reviewing the failures.
    /// </summary>
    private static IQueryable<EmployeeImportError> ApplySorting(IQueryable<EmployeeImportError> query, EmployeeImportErrorListInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.Sorting) && input.Sorting.Contains("desc", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(e => e.RowNumber);
        }

        return query.OrderBy(e => e.RowNumber);
    }

    private static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, PagedResultRequestDto input)
    {
        return query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount <= 0 ? 10 : input.MaxResultCount);
    }
}
