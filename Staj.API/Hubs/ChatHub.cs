// Staj.API/Hubs/ChatHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Staj.API.Hubs;

// SignalR chat hub'ı
// Kullanıcı bağlanınca kendi userId'sine göre gruba katılır
// Alıcıya push için "user_{userId}" grup adı kullanılır
[Authorize]
public class ChatHub : Hub
{
    public const string GroupPrefix = "user_";

    public static string GetUserGroupName(Guid userId) => $"{GroupPrefix}{userId}";

    public override async Task OnConnectedAsync()
    {
        var userIdRaw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdRaw, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdRaw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdRaw, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }
        await base.OnDisconnectedAsync(exception);
    }
}