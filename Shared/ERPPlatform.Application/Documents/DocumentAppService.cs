using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.BlobStoring;
using ERPPlatform.Documents;

namespace ERPPlatform.Application.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Extension { get; set; }
    public long SizeBytes { get; set; }
    public string ContentType { get; set; }
    public Guid? FolderId { get; set; }
}

public class DocumentAppService : ApplicationService
{
    private readonly IRepository<Document, Guid> _documentRepository;
    private readonly IBlobContainer _blobContainer;

    public DocumentAppService(
        IRepository<Document, Guid> documentRepository,
        IBlobContainer blobContainer)
    {
        _documentRepository = documentRepository;
        _blobContainer = blobContainer;
    }

    public async Task<List<DocumentDto>> GetListAsync(Guid? folderId)
    {
        var docs = await _documentRepository.GetListAsync(d => d.FolderId == folderId);
        return docs.Select(d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Extension = d.Extension,
            SizeBytes = d.SizeBytes,
            ContentType = d.ContentType,
            FolderId = d.FolderId
        }).ToList();
    }

    public async Task<DocumentDto> UploadAsync(string title, string extension, long sizeBytes, string contentType, Guid? folderId, byte[] content)
    {
        var blobName = Guid.NewGuid().ToString("N") + extension;

        await _blobContainer.SaveAsync(blobName, content);

        var doc = new Document
        {
            Title = title,
            Extension = extension,
            SizeBytes = sizeBytes,
            ContentType = contentType,
            FolderId = folderId,
            BlobName = blobName
        };

        await _documentRepository.InsertAsync(doc);

        return new DocumentDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Extension = doc.Extension,
            SizeBytes = doc.SizeBytes,
            ContentType = doc.ContentType,
            FolderId = doc.FolderId
        };
    }

    public async Task<byte[]> DownloadAsync(Guid id)
    {
        var doc = await _documentRepository.GetAsync(id);
        return await _blobContainer.GetAllBytesAsync(doc.BlobName);
    }

    public async Task DeleteAsync(Guid id)
    {
        var doc = await _documentRepository.GetAsync(id);
        await _blobContainer.DeleteAsync(doc.BlobName);
        await _documentRepository.DeleteAsync(id);
    }
}
