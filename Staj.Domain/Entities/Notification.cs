// Staj.Domain/Entities/Notification.cs
using Staj.Domain.Common;
using Staj.Domain.Enums;

namespace Staj.Domain.Entities;

// Sistem içi bildirim
public class Notification : BaseEntity
{
    // Bildirimi alacak kullanıcı
    public Guid RecipientUserId { get; set; }
    public ApplicationUser Recipient { get; set; } = null!;

    // Bildirim tipi
    public NotificationType Type { get; set; }

    // Başlık ve içerik metni
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    // Referans kaynağı (ör. "Proposal", "Message", "Review")
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    // Okuma durumu
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}