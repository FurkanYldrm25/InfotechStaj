// Staj.Infrastructure/Services/NotificationService.cs
using Microsoft.AspNetCore.SignalR;
using Staj.Application.Common.Interfaces;
using Staj.Application.Features.Notifications.Dtos;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Infrastructure.Services;

// Bildirimi DB'ye yazar ve SignalR "user_{userId}" grubuna push eder
// THub generic'i sayesinde Infrastructure API'ye referans vermez
public class NotificationService<THub> : INotificationService where THub : Hub
{
    private readonly IApplicationDbContext _context;
    private readonly IHubContext<THub> _hubContext;

    public NotificationService(
        IApplicationDbContext context,
        IHubContext<THub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task CreateAndPushAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string body,
        string? referenceType = null,
        Guid? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title.Trim(),
            Body = body.Trim(),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.ReferenceType,
            notification.ReferenceId,
            notification.IsRead,
            notification.ReadAt,
            notification.CreatedAt);

        var groupName = $"user_{recipientUserId}";
        await _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveNotification", dto, cancellationToken);
    }
}