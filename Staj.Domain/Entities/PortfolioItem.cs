// Staj.Domain/Entities/PortfolioItem.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// Freelancer portfolio kalemi
public class PortfolioItem : BaseEntity
{
    public Guid FreelancerProfileId { get; set; }
    public FreelancerProfile FreelancerProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}