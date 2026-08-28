using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Audit
{
    public class AuditLogEntryDto : EntityDto<Guid>
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = "Updated";
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ChangesJson { get; set; } = "{}";
        public string OldValues { get; set; } = "{}";
        public string NewValues { get; set; } = "{}";
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }

    public class GetAuditLogsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public string? EntityName { get; set; }
        public string? Action { get; set; }
        public string? UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public interface IAuditLogAppService : IApplicationService
    {
        Task<PagedResultDto<AuditLogEntryDto>> GetAuditLogsAsync(GetAuditLogsInput input);
        Task<ListResultDto<AuditLogEntryDto>> GetListAsync();
    }

    public class AuditLogAppService : ApplicationService, IAuditLogAppService
    {
        private readonly IRepository<AuditLogEntry, Guid> _auditRepository;

        public AuditLogAppService(IRepository<AuditLogEntry, Guid> auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<ListResultDto<AuditLogEntryDto>> GetListAsync()
        {
            var result = await GetAuditLogsAsync(new GetAuditLogsInput { MaxResultCount = 100 });
            return new ListResultDto<AuditLogEntryDto>(result.Items);
        }

        public async Task<PagedResultDto<AuditLogEntryDto>> GetAuditLogsAsync(GetAuditLogsInput input)
        {
            var query = await _auditRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var term = input.Filter.ToLower();
                query = query.Where(x => x.EntityName.ToLower().Contains(term) ||
                                         x.UserName.ToLower().Contains(term) ||
                                         x.EntityId.ToLower().Contains(term) ||
                                         x.ChangesJson.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(input.EntityName))
            {
                query = query.Where(x => x.EntityName == input.EntityName);
            }

            if (!string.IsNullOrWhiteSpace(input.Action))
            {
                query = query.Where(x => x.Action == input.Action);
            }

            if (!string.IsNullOrWhiteSpace(input.UserName))
            {
                query = query.Where(x => x.UserName == input.UserName);
            }

            if (input.StartDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= input.StartDate.Value);
            }

            if (input.EndDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= input.EndDate.Value);
            }

            var totalCount = query.Count();
            var items = query.OrderByDescending(l => l.Timestamp)
                             .Skip(input.SkipCount)
                             .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 50)
                             .ToList();

            var dtos = items.Select(l => new AuditLogEntryDto
            {
                Id = l.Id,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                UserName = l.UserName,
                Timestamp = l.Timestamp,
                ChangesJson = l.ChangesJson,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CorrelationId = l.CorrelationId
            }).ToList();

            return new PagedResultDto<AuditLogEntryDto>(totalCount, dtos);
        }
    }
}
