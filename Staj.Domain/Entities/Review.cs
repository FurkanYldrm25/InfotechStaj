// Staj.Domain/Entities/Review.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// Karşılıklı değerlendirme
public class Review : BaseEntity
{
    // Değerlendirmenin kaynağı — hangi teklif/iş üzerinden
    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = null!;

    // Yorumu yazan kullanıcı
    public Guid AuthorUserId { get; set; }
    public ApplicationUser Author { get; set; } = null!;

    // Yorumun hedefi
    public Guid TargetUserId { get; set; }
    public ApplicationUser Target { get; set; } = null!;

    // 1-5 arası puan
    public int Rating { get; set; }

    // Serbest yorum metni
    public string Comment { get; set; } = string.Empty;

    // Admin moderasyonu için görünürlük kontrolü
    public bool IsVisible { get; set; } = true;
}