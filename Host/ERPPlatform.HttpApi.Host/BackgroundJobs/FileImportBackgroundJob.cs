using System;
using System.Threading.Tasks;
using ERPPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.BackgroundJobs;

namespace ERPPlatform.BackgroundJobs;

public class FileImportJobArgs
{
    public string UploadId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string FinalBlobName { get; set; } = string.Empty;
}

/// <summary>
/// Processes a finished upload off the request thread. The actual business parsing
/// (Excel/CSV -> entities, validation, etc.) should be plugged in here. When done
/// it pushes a real-time notification to the user who started the import.
/// </summary>
public class FileImportBackgroundJob : AsyncBackgroundJob<FileImportJobArgs>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public FileImportBackgroundJob(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public override async Task ExecuteAsync(FileImportJobArgs args)
    {
        // TODO: replace this stub with real processing for the relevant module
        // (e.g. read the final blob from the "file-imports" container and import rows).
        await Task.Delay(1500);

        if (!string.IsNullOrEmpty(args.UserId))
        {
            // Prefix "IMPORT_DONE|" lets the client detect completion notifications.
            var message = $"IMPORT_DONE|{args.FileName}";
            await _hubContext.Clients.User(args.UserId)
                .SendAsync("ReceiveNotification", message);
        }
    }
}
