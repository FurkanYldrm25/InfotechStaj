// Staj.Application/Features/Freelancers/Commands/CreateOrUpdateProfile/CreateOrUpdateFreelancerProfileCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.CreateOrUpdateProfile;

// Freelancer profili oluşturur ya da günceller (upsert)
public record CreateOrUpdateFreelancerProfileCommand(
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
    bool IsAvailable
) : IRequest<Result<Guid>>;

