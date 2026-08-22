using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ERPPlatform.Documents;

public class Document : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Title { get; set; }
    public string Extension { get; set; }
    public long SizeBytes { get; set; }
    public string ContentType { get; set; }
    public string BlobName { get; set; }
    public Guid? FolderId { get; set; }
}
