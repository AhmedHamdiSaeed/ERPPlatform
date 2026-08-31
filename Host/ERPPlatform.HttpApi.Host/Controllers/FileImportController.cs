using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ERPPlatform.BackgroundJobs;
using ERPPlatform.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.Users;

namespace ERPPlatform.Controllers;

/// <summary>
/// Isolated blob container for resumable file imports. The [BlobContainerName]
/// attribute makes "file-imports" the physical container name.
/// </summary>
[BlobContainerName("file-imports")]
public class FileImportContainer
{
}

/// <summary>
/// Resumable, chunked file import. A client splits a file into fixed-size chunks,
/// uploads them (with the server tracking which ones it already has), and can resume
/// from the last received chunk after a failure. Once complete the file is processed
/// by a background job and the user gets a real-time notification.
/// </summary>
[Authorize]
[ApiController]
[Route("api/file-import")]
public class FileImportController : AbpControllerBase
{
    public const int MaxChunkBytes = 20_000_000; // safety cap per chunk

    private static readonly HashSet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".xls", ".csv" };

    private readonly IBlobContainer<FileImportContainer> _blobContainer;
    private readonly IBackgroundJobManager _jobManager;
    private readonly ICurrentUser _currentUser;

    public FileImportController(
        IBlobContainer<FileImportContainer> blobContainer,
        IBackgroundJobManager jobManager,
        ICurrentUser currentUser)
    {
        _blobContainer = blobContainer;
        _jobManager = jobManager;
        _currentUser = currentUser;
    }

    // ── Start a new upload session ──────────────────────────────
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartImportInput input)
    {
        if (input == null ||
            string.IsNullOrWhiteSpace(input.FileName) ||
            input.TotalChunks <= 0 ||
            input.TotalSize <= 0)
        {
            return BadRequest("Invalid start request.");
        }

        var ext = Path.GetExtension(input.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            return BadRequest("Unsupported file type. Only .xlsx, .xls and .csv files are allowed.");

        var uploadId = Guid.NewGuid().ToString("N");
        var session = new ImportSession
        {
            UploadId = uploadId,
            FileName = input.FileName,
            ContentType = string.IsNullOrWhiteSpace(input.ContentType)
                ? "application/octet-stream"
                : input.ContentType,
            TotalSize = input.TotalSize,
            TotalChunks = input.TotalChunks,
            ReceivedIndices = new List<int>(),
            UserId = _currentUser.Id?.ToString() ?? string.Empty,
            TenantId = _currentUser.TenantId,
            Status = "Uploading",
            CreatedAt = DateTime.UtcNow
        };

        await _blobContainer.SaveAsync(
            $"{uploadId}/session.json",
            JsonSerializer.SerializeToUtf8Bytes(session),
            overrideExisting: true);

        return Ok(new { uploadId });
    }

    // ── Upload one chunk (multipart) ────────────────────────────
    [HttpPost("chunk")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxChunkBytes)]
    public async Task<IActionResult> UploadChunk([FromForm] UploadChunkInput input)
    {
        if (input?.Chunk == null || input.Chunk.Length == 0)
            return BadRequest("No chunk provided.");

        var sessionBlob = await _blobContainer.GetAllBytesAsync($"{input.UploadId}/session.json");
        if (sessionBlob == null)
            return NotFound("Unknown upload session.");

        var session = JsonSerializer.Deserialize<ImportSession>(sessionBlob);
        if (session == null)
            return NotFound("Corrupt upload session.");

        await using (var chunkStream = input.Chunk.OpenReadStream())
        {
            using var ms = new MemoryStream();
            await chunkStream.CopyToAsync(ms);
            await _blobContainer.SaveAsync(
                $"{input.UploadId}/chunks/{input.Index}",
                ms.ToArray(),
                overrideExisting: true);
        }

        // Record the chunk only if not already counted (also covers resume re-sends).
        if (!session.ReceivedIndices.Contains(input.Index))
        {
            session.ReceivedIndices.Add(input.Index);
            session.ReceivedIndices.Sort();
            await _blobContainer.SaveAsync(
                $"{input.UploadId}/session.json",
                JsonSerializer.SerializeToUtf8Bytes(session),
                overrideExisting: true);
        }

        return Ok(new { received = session.ReceivedIndices.Count, total = session.TotalChunks });
    }

    // ── Query upload progress / which chunks are still missing ──
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string uploadId)
    {
        if (string.IsNullOrWhiteSpace(uploadId))
            return BadRequest("uploadId is required.");

        var sessionBlob = await _blobContainer.GetAllBytesAsync($"{uploadId}/session.json");
        if (sessionBlob == null)
            return NotFound("Unknown upload session.");

        var session = JsonSerializer.Deserialize<ImportSession>(sessionBlob);
        if (session == null)
            return NotFound("Corrupt upload session.");

        var percent = session.TotalChunks == 0
            ? 0
            : (int)Math.Round(session.ReceivedIndices.Count * 100.0 / session.TotalChunks);

        return Ok(new
        {
            uploadId,
            receivedIndices = session.ReceivedIndices,
            totalChunks = session.TotalChunks,
            percent,
            status = session.Status
        });
    }

    // ── Finish: reassemble chunks, enqueue processing job ──────
    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteImportInput input)
    {
        var sessionBlob = await _blobContainer.GetAllBytesAsync($"{input.UploadId}/session.json");
        if (sessionBlob == null)
            return NotFound("Unknown upload session.");

        var session = JsonSerializer.Deserialize<ImportSession>(sessionBlob);
        if (session == null)
            return NotFound("Corrupt upload session.");

        if (session.ReceivedIndices.Count != session.TotalChunks)
        {
            return BadRequest(new
            {
                message = "Some chunks are still missing.",
                received = session.ReceivedIndices.Count,
                total = session.TotalChunks
            });
        }

        // Concatenate chunks in order into the final blob.
        using var output = new MemoryStream();
        for (var i = 0; i < session.TotalChunks; i++)
        {
            var chunk = await _blobContainer.GetAllBytesAsync($"{input.UploadId}/chunks/{i}");
            if (chunk == null)
                return BadRequest(new { message = $"Missing chunk {i}." });
            await output.WriteAsync(chunk, 0, chunk.Length);
        }

        var finalBytes = output.ToArray();
        if (!IsValidExcelOrCsv(finalBytes, session.FileName))
            return BadRequest(new { message = "The uploaded file is not a valid Excel or CSV file." });

        var finalBlobName = $"{input.UploadId}/final/{session.FileName}";
        await _blobContainer.SaveAsync(finalBlobName, finalBytes, overrideExisting: true);

        session.Status = "Processing";
        await _blobContainer.SaveAsync(
            $"{input.UploadId}/session.json",
            JsonSerializer.SerializeToUtf8Bytes(session),
            overrideExisting: true);

        await _jobManager.EnqueueAsync(new FileImportJobArgs
        {
            UploadId = input.UploadId,
            FileName = session.FileName,
            ContentType = session.ContentType,
            TotalSize = session.TotalSize,
            UserId = session.UserId,
            TenantId = session.TenantId,
            FinalBlobName = finalBlobName
        });

        return Ok(new { enqueued = true });
    }

    /// <summary>
    /// Server-side defense: verifies the actual bytes match the claimed Excel/CSV type,
    /// so a renamed/forged file (e.g. an .exe named .xlsx) is rejected even if the
    /// extension check passed.
    /// </summary>
    private static bool IsValidExcelOrCsv(byte[] data, string fileName)
    {
        if (data == null || data.Length == 0) return false;
        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        // .xlsx / .xlsm are ZIP-based Office Open XML.
        if (ext == ".xlsx" || ext == ".xlsm")
            return data.Length >= 4 &&
                   data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04;

        // .xls is an OLE2 Compound File.
        if (ext == ".xls")
            return data.Length >= 8 &&
                   data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
                   data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1;

        // .csv is plain text — reject obvious binary blobs (e.g. "MZ" PE executables).
        if (ext == ".csv")
            return !(data.Length >= 2 && data[0] == 0x4D && data[1] == 0x5A);

        return false;
    }
}

// ──────────────────────────────────────────────────────────────
// Models
// ──────────────────────────────────────────────────────────────

public class ImportSession
{
    public string UploadId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public int TotalChunks { get; set; }
    public List<int> ReceivedIndices { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StartImportInput
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public int TotalChunks { get; set; }
}

public class UploadChunkInput
{
    public string UploadId { get; set; } = string.Empty;
    public int Index { get; set; }
    public IFormFile Chunk { get; set; } = default!;
}

public class CompleteImportInput
{
    public string UploadId { get; set; } = string.Empty;
}
