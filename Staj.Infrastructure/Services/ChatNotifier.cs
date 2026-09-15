// Staj.Infrastructure/Services/ChatNotifier.cs
using Microsoft.AspNetCore.SignalR;
using Staj.Application.Common.Interfaces;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Infrastructure.Services;

// SignalR üzerinden bildirim gönderen implementasyon
// IHubContext<THub> ile Hub'a bağımlı olmadan mesaj push edebiliriz
public class ChatNotifier<THub> : IChatNotifier where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;

    public ChatNotifier(IHubContext<THub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyMessageAsync(Guid recipientUserId, MessageDto message, CancellationToken cancellationToken = default)
    {
        var groupName = $"user_{recipientUserId}";
        return _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveMessage", message, cancellationToken);
    }

    public Task NotifyReadAsync(Guid recipientUserId, Guid conversationId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        var groupName = $"user_{recipientUserId}";
        return _hubContext.Clients.Group(groupName)
            .SendAsync("ConversationRead", new { conversationId, readAt }, cancellationToken);
    }
}