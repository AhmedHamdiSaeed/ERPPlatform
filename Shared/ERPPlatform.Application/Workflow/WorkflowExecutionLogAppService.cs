using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Workflow
{
    public class WorkflowExecutionStepDto : EntityDto<Guid>
    {
        public Guid WorkflowExecutionLogId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "Passed";
        public string Details { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class WorkflowExecutionLogDto : EntityDto<Guid>
    {
        public string ExecutionCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public string TriggeredBy { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Status { get; set; } = "Running";
        public List<WorkflowExecutionStepDto> Steps { get; set; } = new();
    }

    public class CreateWorkflowExecutionLogDto
    {
        public string ExecutionCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public string TriggeredBy { get; set; } = string.Empty;
    }

    public class AppendExecutionStepDto
    {
        public string StepName { get; set; } = string.Empty;
        public string Status { get; set; } = "Passed";
        public string Details { get; set; } = string.Empty;
    }

    public interface IWorkflowExecutionLogAppService : IApplicationService
    {
        Task<ListResultDto<WorkflowExecutionLogDto>> GetListAsync(string? status = null, int maxResultCount = 50);
        Task<WorkflowExecutionLogDto> GetAsync(Guid id);
        Task<WorkflowExecutionLogDto> StartAsync(CreateWorkflowExecutionLogDto input);
        Task<WorkflowExecutionLogDto> AppendStepAsync(Guid id, AppendExecutionStepDto input);
        Task<WorkflowExecutionLogDto> CompleteAsync(Guid id, string status = "Completed");
        Task DeleteAsync(Guid id);
    }

    /// <summary>
    /// Execution history for workflow runs. Also the write-side entry point:
    /// StartAsync -> AppendStepAsync (per node) -> CompleteAsync.
    /// </summary>
    public class WorkflowExecutionLogAppService : ApplicationService, IWorkflowExecutionLogAppService
    {
        private readonly IRepository<WorkflowExecutionLog, Guid> _logRepository;
        private readonly IRepository<WorkflowExecutionStep, Guid> _stepRepository;

        public WorkflowExecutionLogAppService(
            IRepository<WorkflowExecutionLog, Guid> logRepository,
            IRepository<WorkflowExecutionStep, Guid> stepRepository)
        {
            _logRepository = logRepository;
            _stepRepository = stepRepository;
        }

        public async Task<ListResultDto<WorkflowExecutionLogDto>> GetListAsync(
            string? status = null, int maxResultCount = 50)
        {
            if (maxResultCount <= 0) maxResultCount = 50;
            if (maxResultCount > 500) maxResultCount = 500;

            var logQuery = await _logRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(status))
            {
                logQuery = logQuery.Where(l => l.Status == status);
            }

            logQuery = logQuery.OrderByDescending(l => l.StartTime).Take(maxResultCount);

            var logs = await AsyncExecuter.ToListAsync(logQuery);
            var logIds = logs.Select(l => l.Id).ToList();

            var stepQuery = await _stepRepository.GetQueryableAsync();
            var steps = await AsyncExecuter.ToListAsync(
                stepQuery.Where(s => logIds.Contains(s.WorkflowExecutionLogId)));

            return new ListResultDto<WorkflowExecutionLogDto>(
                logs.Select(l => MapToDto(l, steps.Where(s => s.WorkflowExecutionLogId == l.Id))).ToList());
        }

        public async Task<WorkflowExecutionLogDto> GetAsync(Guid id)
        {
            var log = await _logRepository.GetAsync(id);

            var stepQuery = await _stepRepository.GetQueryableAsync();
            var steps = await AsyncExecuter.ToListAsync(
                stepQuery.Where(s => s.WorkflowExecutionLogId == id).OrderBy(s => s.Order));

            return MapToDto(log, steps);
        }

        public async Task<WorkflowExecutionLogDto> StartAsync(CreateWorkflowExecutionLogDto input)
        {
            var log = new WorkflowExecutionLog
            {
                ExecutionCode = string.IsNullOrWhiteSpace(input.ExecutionCode)
                    ? $"EXEC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
                    : input.ExecutionCode,
                WorkflowName = input.WorkflowName,
                WorkflowDefinitionId = input.WorkflowDefinitionId,
                TriggeredBy = input.TriggeredBy,
                StartTime = DateTime.UtcNow,
                Status = "Running",
                Duration = "0s"
            };

            await _logRepository.InsertAsync(log);
            return MapToDto(log, Enumerable.Empty<WorkflowExecutionStep>());
        }

        public async Task<WorkflowExecutionLogDto> AppendStepAsync(Guid id, AppendExecutionStepDto input)
        {
            var log = await _logRepository.GetAsync(id);

            var stepQuery = await _stepRepository.GetQueryableAsync();
            var existingSteps = await AsyncExecuter.ToListAsync(
                stepQuery.Where(s => s.WorkflowExecutionLogId == id));

            var step = new WorkflowExecutionStep
            {
                WorkflowExecutionLogId = id,
                StepName = input.StepName,
                Status = input.Status,
                Details = input.Details,
                Timestamp = DateTime.UtcNow,
                Order = existingSteps.Count
            };

            await _stepRepository.InsertAsync(step);

            var steps = existingSteps.Append(step).OrderBy(s => s.Order);
            log.Duration = FormatDuration(DateTime.UtcNow - log.StartTime);
            await _logRepository.UpdateAsync(log);

            return MapToDto(log, steps);
        }

        public async Task<WorkflowExecutionLogDto> CompleteAsync(Guid id, string status = "Completed")
        {
            var log = await _logRepository.GetAsync(id);

            log.Status = status;
            log.EndTime = DateTime.UtcNow;
            log.Duration = FormatDuration(log.EndTime.Value - log.StartTime);

            await _logRepository.UpdateAsync(log);

            var stepQuery = await _stepRepository.GetQueryableAsync();
            var steps = await AsyncExecuter.ToListAsync(
                stepQuery.Where(s => s.WorkflowExecutionLogId == id).OrderBy(s => s.Order));

            return MapToDto(log, steps);
        }

        public async Task DeleteAsync(Guid id)
        {
            var stepQuery = await _stepRepository.GetQueryableAsync();
            var steps = await AsyncExecuter.ToListAsync(
                stepQuery.Where(s => s.WorkflowExecutionLogId == id));

            foreach (var step in steps)
            {
                await _stepRepository.DeleteAsync(step);
            }

            await _logRepository.DeleteAsync(id);
        }

        private static string FormatDuration(TimeSpan span)
        {
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h {span.Minutes}m";
            if (span.TotalMinutes >= 1) return $"{span.Minutes}m {span.Seconds}s";
            return $"{span.Seconds}s";
        }

        private static WorkflowExecutionLogDto MapToDto(
            WorkflowExecutionLog log, IEnumerable<WorkflowExecutionStep> steps)
        {
            return new WorkflowExecutionLogDto
            {
                Id = log.Id,
                ExecutionCode = log.ExecutionCode,
                WorkflowName = log.WorkflowName,
                WorkflowDefinitionId = log.WorkflowDefinitionId,
                TriggeredBy = log.TriggeredBy,
                StartTime = log.StartTime,
                EndTime = log.EndTime,
                Duration = log.Duration,
                Status = log.Status,
                Steps = steps.OrderBy(s => s.Order).Select(s => new WorkflowExecutionStepDto
                {
                    Id = s.Id,
                    WorkflowExecutionLogId = s.WorkflowExecutionLogId,
                    StepName = s.StepName,
                    Timestamp = s.Timestamp,
                    Status = s.Status,
                    Details = s.Details,
                    Order = s.Order
                }).ToList()
            };
        }
    }
}
