using System;
using System.Threading.Tasks;
using ERPPlatform.Application.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace ERPPlatform.Controllers;

[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentController : AbpControllerBase
{
    private readonly DocumentAppService _documentAppService;
    private readonly FolderAppService _folderAppService;

    public DocumentController(
        DocumentAppService documentAppService,
        FolderAppService folderAppService)
    {
        _documentAppService = documentAppService;
        _folderAppService = folderAppService;
    }

    // ── Folders ──────────────────────────────────────────────

    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders([FromQuery] Guid? parentId)
    {
        var result = await _folderAppService.GetListAsync(parentId);
        return Ok(result);
    }

    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
    {
        var result = await _folderAppService.CreateAsync(request.Name, request.ParentId);
        return Ok(result);
    }

    [HttpDelete("folders/{id}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        await _folderAppService.DeleteAsync(id);
        return NoContent();
    }

    // ── Documents ────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] Guid? folderId)
    {
        var result = await _documentAppService.GetListAsync(folderId);
        return Ok(result);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)] // 100 MB
    public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
    {
        if (dto?.File == null || dto.File.Length == 0)
            return BadRequest("No file provided.");

        var file = dto.File;
        var ext = System.IO.Path.GetExtension(file.FileName);
        var title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

        using var ms = new System.IO.MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var result = await _documentAppService.UploadAsync(
            title, ext, file.Length, file.ContentType, dto.FolderId, bytes);

        return Ok(result);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var bytes = await _documentAppService.DownloadAsync(id);
        return File(bytes, "application/octet-stream");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _documentAppService.DeleteAsync(id);
        return NoContent();
    }
}

public class CreateFolderRequest
{
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
}

public class UploadDocumentDto
{
    public IFormFile File { get; set; }
    public Guid? FolderId { get; set; }
}
