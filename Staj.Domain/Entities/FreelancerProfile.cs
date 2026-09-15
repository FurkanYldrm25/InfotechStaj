// Staj.Domain/Entities/FreelancerProfile.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// Mühendis/yazılımcı profili
public class FreelancerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? Title { get; set; }
    public string? Bio { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? HourlyRateMin { get; set; }
    public decimal? HourlyRateMax { get; set; }
    public string? Currency { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CvUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    public ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();
    public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
}