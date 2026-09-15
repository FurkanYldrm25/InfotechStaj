// Staj.Domain/Entities/Message.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// Tekil mesaj
public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}