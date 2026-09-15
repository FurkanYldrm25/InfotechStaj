// Staj.Domain/Entities/FreelancerSkill.cs
namespace Staj.Domain.Entities;

// Freelancer ↔ Skill çok-çoğa bağlantısı
public class FreelancerSkill
{
    public Guid FreelancerProfileId { get; set; }
    public FreelancerProfile FreelancerProfile { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int? ProficiencyLevel { get; set; } // 1-5 arası
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}