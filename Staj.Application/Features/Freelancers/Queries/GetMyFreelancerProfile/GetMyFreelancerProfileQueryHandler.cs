// Staj.Application/Features/Freelancers/Queries/GetMyFreelancerProfile/GetMyFreelancerProfileQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Freelancers.Queries.GetMyFreelancerProfile;

public class GetMyFreelancerProfileQueryHandler
    : IRequestHandler<GetMyFreelancerProfileQuery, Result<FreelancerProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyFreelancerProfileQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FreelancerProfileDto>> Handle(
        GetMyFreelancerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<FreelancerProfileDto>.Fail("Yetkisiz erişim.");

        var profile = await _context.FreelancerProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.FreelancerSkills).ThenInclude(fs => fs.Skill)
            .Include(p => p.PortfolioItems)
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
            return Result<FreelancerProfileDto>.Fail("Profil bulunamadı.");

        // Kendi profilini görüntülerken de rating özeti gelsin
        var (average, count, recent) = await LoadRatingAsync(_context, profile.UserId, cancellationToken);

        var dto = MapToDto(profile, average, count, recent);
        return Result<FreelancerProfileDto>.Ok(dto);
    }

    // Rating özetini ve son N yorumu tek yerden çek
    internal static async Task<(double average, int count, List<ReviewDto> recent)> LoadRatingAsync(
        IApplicationDbContext context,
        Guid targetUserId,
        CancellationToken cancellationToken,
        int recentTake = 5)
    {
        var visibleReviews = context.Reviews
            .AsNoTracking()
            .Where(r => r.TargetUserId == targetUserId && r.IsVisible);

        var count = await visibleReviews.CountAsync(cancellationToken);
        var average = count == 0
            ? 0
            : await visibleReviews.AverageAsync(r => (double)r.Rating, cancellationToken);

        var recent = await visibleReviews
            .Include(r => r.Author)
            .Include(r => r.Target)
            .Include(r => r.Proposal).ThenInclude(p => p.JobPost)
            .OrderByDescending(r => r.CreatedAt)
            .Take(recentTake)
            .Select(r => new ReviewDto(
                r.Id,
                r.ProposalId,
                r.Proposal.JobPostId,
                r.Proposal.JobPost != null ? r.Proposal.JobPost.Title : null,
                r.AuthorUserId,
                r.Author.FirstName,
                r.Author.LastName,
                r.Author.ProfileImageUrl,
                r.TargetUserId,
                r.Target.FirstName,
                r.Target.LastName,
                r.Rating,
                r.Comment,
                r.IsVisible,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        return (Math.Round(average, 2), count, recent);
    }

    internal static FreelancerProfileDto MapToDto(
        Domain.Entities.FreelancerProfile p,
        double averageRating,
        int reviewCount,
        List<ReviewDto> recentReviews) =>
        new(
            p.Id, p.UserId,
            p.User.FirstName, p.User.LastName, p.User.Email ?? string.Empty,
            p.User.ProfileImageUrl,
            p.Title, p.Bio, p.ExperienceYears,
            p.HourlyRateMin, p.HourlyRateMax, p.Currency,
            p.Country, p.City,
            p.CvUrl, p.LinkedInUrl, p.GitHubUrl, p.WebsiteUrl,
            p.IsAvailable,
            p.FreelancerSkills
                .OrderBy(fs => fs.Skill.Name)
                .Select(fs => new FreelancerSkillDto(fs.SkillId, fs.Skill.Name, fs.Skill.Slug, fs.ProficiencyLevel))
                .ToList(),
            p.PortfolioItems
                .OrderBy(pi => pi.DisplayOrder).ThenBy(pi => pi.CreatedAt)
                .Select(pi => new PortfolioItemDto(pi.Id, pi.Title, pi.Description, pi.ProjectUrl, pi.ImageUrl, pi.DisplayOrder))
                .ToList(),
            averageRating,
            reviewCount,
            recentReviews);
}