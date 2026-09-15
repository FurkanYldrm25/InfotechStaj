// Staj.Application/Common/Interfaces/IChatNotifier.cs
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Common.Interfaces;

// Application katmanının SignalR'a bağlı kalmadan bildirim yollayabilmesi için soyutlama
public interface IChatNotifier
{
    // Alıcının "user_{userId}" grubuna yeni mesajı push'lar
    Task NotifyMessageAsync(Guid recipientUserId, MessageDto message, CancellationToken cancellationToken = default);

    // Karşı tarafa "mesajların okundu" bildirimi
    Task NotifyReadAsync(Guid recipientUserId, Guid conversationId, DateTime readAt, CancellationToken cancellationToken = default);
}