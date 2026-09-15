// Staj.Domain/Entities/Conversation.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// İki kullanıcı arasındaki mesajlaşma başlığı
// Participant1Id < Participant2Id kuralı ile normalize edilir
public class Conversation : BaseEntity
{
    public Guid Participant1Id { get; set; }
    public ApplicationUser Participant1 { get; set; } = null!;

    public Guid Participant2Id { get; set; }
    public ApplicationUser Participant2 { get; set; } = null!;

    // Listede önizleme için
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public Guid? LastMessageSenderId { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}