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
    }

    public interface IAuditLogAppService : IApplicationService
    {
        Task<ListResultDto<AuditLogEntryDto>> GetAuditLogsAsync();
    }

    public class AuditLogAppService : ApplicationService, IAuditLogAppService
    {
        private readonly IRepository<AuditLogEntry, Guid> _auditRepository;

        public AuditLogAppService(IRepository<AuditLogEntry, Guid> auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<ListResultDto<AuditLogEntryDto>> GetAuditLogsAsync()
        {
            var logs = await _auditRepository.GetListAsync();
            var dtos = logs.Select(l => new AuditLogEntryDto
            {
                Id = l.Id,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                UserName = l.UserName,
                Timestamp = l.Timestamp,
                ChangesJson = l.ChangesJson
            }).OrderByDescending(l => l.Timestamp).ToList();

            return new ListResultDto<AuditLogEntryDto>(dtos);
        }
    }
}
