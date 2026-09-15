// Staj.Application/Features/Notifications/Dtos/NotificationDto.cs
using Staj.Domain.Enums;

namespace Staj.Application.Features.Notifications.Dtos;

// Bildirim DTO
public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Body,
    string? ReferenceType,
    Guid? ReferenceId,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

// Okunmamış sayacı
public record UnreadCountDto(int UnreadCount);