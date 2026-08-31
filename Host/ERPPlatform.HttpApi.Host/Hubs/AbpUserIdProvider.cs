using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.Security.Claims;

namespace ERPPlatform.Hubs;

/// <summary>
/// Maps SignalR connections to ABP user IDs so that
/// Clients.User(userId) delivers to the correct user.
/// Uses the NameIdentifier claim (ABP's CurrentUser.Id as string).
/// </summary>
public class AbpUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // ABP sets the NameIdentifier claim to the user's GUID ID
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? connection.User?.FindFirst(AbpClaimTypes.UserId)?.Value;
    }
}
