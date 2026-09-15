// Staj.Application/Features/Freelancers/Dtos/FreelancerProfileDto.cs
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Freelancers.Dtos;

// Freelancer profil DTO'su
public record FreelancerProfileDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? ProfileImageUrl,
    string? Title,
    string? Bio,
    int? ExperienceYears,
    decimal? HourlyRateMin,
    decimal? HourlyRateMax,
    string? Currency,
    string? Country,
    string? City,
    string? CvUrl,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WebsiteUrl,
    bool IsAvailable,
    List<FreelancerSkillDto> Skills,
    List<PortfolioItemDto> PortfolioItems,
    double AverageRating,
    int ReviewCount,
    List<ReviewDto> RecentReviews
);

// Freelancer arama listesi için kısa DTO
public record FreelancerListItemDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? ProfileImageUrl,
    string? Title,
    int? ExperienceYears,
    decimal? HourlyRateMin,
    decimal? HourlyRateMax,
    string? Currency,
    string? Country,
    string? City,
    bool IsAvailable,
    List<string> Skills
);

// Skill özet DTO (freelancer içinde)
public record FreelancerSkillDto(Guid SkillId, string Name, string Slug, int? ProficiencyLevel);

// Portfolio DTO
public record PortfolioItemDto(
    Guid Id,
    string Title,
    string? Description,
    string? ProjectUrl,
    string? ImageUrl,
    int DisplayOrder
);