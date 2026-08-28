using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Volo.Abp.Users;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.HttpApi.Host.Auditing
{
    public class AuditingInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUser _currentUser;

        public AuditingInterceptor(
            IHttpContextAccessor httpContextAccessor,
            ICurrentUser currentUser)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUser = currentUser;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
            {
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var dbContext = eventData.Context;
            var entries = dbContext.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLogEntry &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            if (!entries.Any())
            {
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "System Internal";
            var userName = _currentUser.UserName ?? _currentUser.Email ?? "System Admin";
            var correlationId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();

            var auditEntries = new List<AuditLogEntry>();

            foreach (var entry in entries)
            {
                var entityName = entry.Entity.GetType().Name;
                var entityId = entry.Property("Id")?.CurrentValue?.ToString() ?? Guid.NewGuid().ToString();

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        continue;
                    }

                    var propertyName = prop.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = prop.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = prop.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (prop.IsModified)
                            {
                                oldValues[propertyName] = prop.OriginalValue;
                                newValues[propertyName] = prop.CurrentValue;
                            }
                            break;
                    }
                }

                var actionStr = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => "Updated"
                };

                var changesPayload = new
                {
                    Old = oldValues,
                    New = newValues
                };

                var auditLog = new AuditLogEntry
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = actionStr,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow,
                    ChangesJson = JsonSerializer.Serialize(changesPayload),
                    OldValues = JsonSerializer.Serialize(oldValues),
                    NewValues = JsonSerializer.Serialize(newValues),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CorrelationId = correlationId
                };

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Any())
            {
                dbContext.Set<AuditLogEntry>().AddRange(auditEntries);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
