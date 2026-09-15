// Staj.Application/Common/Interfaces/INotificationService.cs
using Staj.Domain.Enums;

namespace Staj.Application.Common.Interfaces;

// Bildirimi hem DB'ye yazan hem SignalR ile push eden servis
// Handler'lar bunu çağırır; SignalR'a doğrudan bağımlılık yok
public interface INotificationService
{
    Task CreateAndPushAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string body,
        string? referenceType = null,
        Guid? referenceId = null,
        CancellationToken cancellationToken = default);
}