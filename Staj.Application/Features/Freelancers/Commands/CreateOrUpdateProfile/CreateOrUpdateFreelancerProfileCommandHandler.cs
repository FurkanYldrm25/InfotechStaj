// Staj.Application/Features/Freelancers/Commands/CreateOrUpdateProfile/CreateOrUpdateFreelancerProfileCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Freelancers.Commands.CreateOrUpdateProfile;

// Freelancer profil upsert işleyicisi
public class CreateOrUpdateFreelancerProfileCommandHandler
    : IRequestHandler<CreateOrUpdateFreelancerProfileCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateOrUpdateFreelancerProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrUpdateFreelancerProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
        {
            profile = new FreelancerProfile
            {
                UserId = userId.Value,
                Title = request.Title?.Trim(),
                Bio = request.Bio?.Trim(),
                ExperienceYears = request.ExperienceYears,
                HourlyRateMin = request.HourlyRateMin,
                HourlyRateMax = request.HourlyRateMax,
                Currency = request.Currency?.Trim(),
                Country = request.Country?.Trim(),
                City = request.City?.Trim(),
                CvUrl = request.CvUrl?.Trim(),
                LinkedInUrl = request.LinkedInUrl?.Trim(),
                GitHubUrl = request.GitHubUrl?.Trim(),
                WebsiteUrl = request.WebsiteUrl?.Trim(),
                IsAvailable = request.IsAvailable
            };
            _context.FreelancerProfiles.Add(profile);
        }
        else
        {
            profile.Title = request.Title?.Trim();
            profile.Bio = request.Bio?.Trim();
            profile.ExperienceYears = request.ExperienceYears;
            profile.HourlyRateMin = request.HourlyRateMin;
            profile.HourlyRateMax = request.HourlyRateMax;
            profile.Currency = request.Currency?.Trim();
            profile.Country = request.Country?.Trim();
            profile.City = request.City?.Trim();
            profile.CvUrl = request.CvUrl?.Trim();
            profile.LinkedInUrl = request.LinkedInUrl?.Trim();
            profile.GitHubUrl = request.GitHubUrl?.Trim();
            profile.WebsiteUrl = request.WebsiteUrl?.Trim();
            profile.IsAvailable = request.IsAvailable;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(profile.Id, "Freelancer profili kaydedildi.");
    }
}