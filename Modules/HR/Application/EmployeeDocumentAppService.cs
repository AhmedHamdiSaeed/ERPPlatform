using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class EmployeeDocumentDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "NationalId";
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsVerified { get; set; }
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public int DaysUntilExpiration { get; set; }
    }

    public class CreateUpdateEmployeeDocumentDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "NationalId"; // NationalId, Passport, Contract, Medical, Certificate, Degree, Tax, Visa, Warning, Other
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public interface IEmployeeDocumentAppService : ICrudAppService<EmployeeDocumentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDocumentDto>
    {
        Task<List<EmployeeDocumentDto>> GetDocumentsByEmployeeIdAsync(Guid employeeId);
        Task<List<EmployeeDocumentDto>> GetExpiringDocumentsAsync(int daysThreshold = 30);
        Task<EmployeeDocumentDto> VerifyDocumentAsync(Guid id, string verifiedBy);
    }

    public class EmployeeDocumentAppService : CrudAppService<EmployeeDocument, EmployeeDocumentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateEmployeeDocumentDto>, IEmployeeDocumentAppService
    {
        public EmployeeDocumentAppService(IRepository<EmployeeDocument, Guid> repository) : base(repository)
        {
        }

        public async Task<List<EmployeeDocumentDto>> GetDocumentsByEmployeeIdAsync(Guid employeeId)
        {
            var list = await Repository.GetListAsync(d => d.EmployeeId == employeeId);
            return list.OrderByDescending(d => d.CreationTime).Select(MapEntityToDto).ToList();
        }

        public async Task<List<EmployeeDocumentDto>> GetExpiringDocumentsAsync(int daysThreshold = 30)
        {
            var now = DateTime.UtcNow;
            var target = now.AddDays(daysThreshold);
            var list = await Repository.GetListAsync(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value <= target);
            return list.OrderBy(d => d.ExpiryDate).Select(MapEntityToDto).ToList();
        }

        public async Task<EmployeeDocumentDto> VerifyDocumentAsync(Guid id, string verifiedBy)
        {
            var doc = await Repository.GetAsync(id);
            doc.IsVerified = true;
            doc.VerifiedBy = verifiedBy;
            doc.VerificationDate = DateTime.UtcNow;
            await Repository.UpdateAsync(doc);
            return MapEntityToDto(doc);
        }

        private static EmployeeDocumentDto MapEntityToDto(EmployeeDocument d)
        {
            var now = DateTime.UtcNow;
            bool expired = d.ExpiryDate.HasValue && d.ExpiryDate.Value.Date < now.Date;
            int days = 0;
            if (d.ExpiryDate.HasValue)
            {
                days = (int)(d.ExpiryDate.Value.Date - now.Date).TotalDays;
            }

            return new EmployeeDocumentDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.EmployeeName,
                DocumentType = d.DocumentType,
                DocumentTitle = d.DocumentTitle,
                DocumentNumber = d.DocumentNumber,
                FileUrl = d.FileUrl,
                FileName = d.FileName,
                FileSize = d.FileSize,
                IssueDate = d.IssueDate,
                ExpiryDate = d.ExpiryDate,
                IsVerified = d.IsVerified,
                VerifiedBy = d.VerifiedBy,
                VerificationDate = d.VerificationDate,
                Notes = d.Notes,
                IsExpired = expired,
                DaysUntilExpiration = days
            };
        }

        protected override Task<EmployeeDocument> MapToEntityAsync(CreateUpdateEmployeeDocumentDto input)
        {
            return Task.FromResult(new EmployeeDocument
            {
                EmployeeId = input.EmployeeId,
                EmployeeName = input.EmployeeName,
                DocumentType = input.DocumentType,
                DocumentTitle = input.DocumentTitle,
                DocumentNumber = input.DocumentNumber,
                FileUrl = input.FileUrl,
                FileName = input.FileName,
                FileSize = input.FileSize,
                IssueDate = input.IssueDate,
                ExpiryDate = input.ExpiryDate,
                Notes = input.Notes,
                IsVerified = false
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateEmployeeDocumentDto input, EmployeeDocument entity)
        {
            entity.EmployeeId = input.EmployeeId;
            entity.EmployeeName = input.EmployeeName;
            entity.DocumentType = input.DocumentType;
            entity.DocumentTitle = input.DocumentTitle;
            entity.DocumentNumber = input.DocumentNumber;
            if (!string.IsNullOrWhiteSpace(input.FileUrl)) entity.FileUrl = input.FileUrl;
            if (!string.IsNullOrWhiteSpace(input.FileName)) entity.FileName = input.FileName;
            if (input.FileSize > 0) entity.FileSize = input.FileSize;
            entity.IssueDate = input.IssueDate;
            entity.ExpiryDate = input.ExpiryDate;
            entity.Notes = input.Notes;
            return Task.CompletedTask;
        }

        protected override Task<EmployeeDocumentDto> MapToGetOutputDtoAsync(EmployeeDocument entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
