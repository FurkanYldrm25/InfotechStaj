// Staj.Domain/Entities/CategorySkill.cs
namespace Staj.Domain.Entities;

// Category ve Skill arası çok-çoğa bağlantı tablosu
public class CategorySkill
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}