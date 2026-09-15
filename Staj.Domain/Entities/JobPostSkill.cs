// Staj.Domain/Entities/JobPostSkill.cs
namespace Staj.Domain.Entities;

// JobPost ↔ Skill çok-çoğa bağlantısı
public class JobPostSkill
{
    public Guid JobPostId { get; set; }
    public JobPost JobPost { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}