// Staj.Domain/Entities/JobPost.cs
using Staj.Domain.Common;
using Staj.Domain.Enums;

namespace Staj.Domain.Entities;

// İş ilanı
public class JobPost : BaseEntity
{
    public Guid ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? Currency { get; set; }

    public WorkMode WorkMode { get; set; } = WorkMode.Remote;
    public int? DurationDays { get; set; }

    public string? Country { get; set; }
    public string? City { get; set; }

    public JobPostStatus Status { get; set; } = JobPostStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<JobPostSkill> JobPostSkills { get; set; } = new List<JobPostSkill>();
}