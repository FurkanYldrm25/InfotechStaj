// Staj.Application/Common/Interfaces/IApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Staj.Domain.Entities;

namespace Staj.Application.Common.Interfaces;

// Application katmanının DbContext üzerinden erişebileceği soyutlama
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<CategorySkill> CategorySkills { get; }
    DbSet<FreelancerProfile> FreelancerProfiles { get; }
    DbSet<ClientProfile> ClientProfiles { get; }
    DbSet<FreelancerSkill> FreelancerSkills { get; }
    DbSet<PortfolioItem> PortfolioItems { get; }
    DbSet<JobPost> JobPosts { get; }
    DbSet<JobPostSkill> JobPostSkills { get; }
    DbSet<Proposal> Proposals { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}