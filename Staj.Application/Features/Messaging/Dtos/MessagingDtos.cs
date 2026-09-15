// Staj.Application/Features/Messaging/Dtos/MessagingDtos.cs
namespace Staj.Application.Features.Messaging.Dtos;

// Mesaj DTO'su
public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderFirstName,
    string SenderLastName,
    string? SenderProfileImageUrl,
    string Content,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

// Konuşma listesi kalemi (kullanıcının kendi görüşü açısından)
public record ConversationListItemDto(
    Guid Id,
    Guid OtherUserId,
    string OtherFirstName,
    string OtherLastName,
    string? OtherProfileImageUrl,
    string? LastMessagePreview,
    Guid? LastMessageSenderId,
    DateTime? LastMessageAt,
    int UnreadCount
);

// Toplam okunmamış mesaj sayısı DTO'su
public record UnreadCountDto(int TotalUnread);