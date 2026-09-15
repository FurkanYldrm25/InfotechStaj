// Staj.Application/Features/Clients/Commands/CreateOrUpdateProfile/CreateOrUpdateClientProfileCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Clients.Commands.CreateOrUpdateProfile;

public class CreateOrUpdateClientProfileCommandHandler
    : IRequestHandler<CreateOrUpdateClientProfileCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateOrUpdateClientProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrUpdateClientProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<Guid>.Fail("Yetkisiz erişim.");

        var profile = await _context.ClientProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
        {
            profile = new ClientProfile
            {
                UserId = userId.Value,
                CompanyName = request.CompanyName?.Trim(),
                Industry = request.Industry?.Trim(),
                About = request.About?.Trim(),
                WebsiteUrl = request.WebsiteUrl?.Trim(),
                Country = request.Country?.Trim(),
                City = request.City?.Trim(),
                ContactPreference = request.ContactPreference,
                ContactPhone = request.ContactPhone?.Trim()
            };
            _context.ClientProfiles.Add(profile);
        }
        else
        {
            profile.CompanyName = request.CompanyName?.Trim();
            profile.Industry = request.Industry?.Trim();
            profile.About = request.About?.Trim();
            profile.WebsiteUrl = request.WebsiteUrl?.Trim();
            profile.Country = request.Country?.Trim();
            profile.City = request.City?.Trim();
            profile.ContactPreference = request.ContactPreference;
            profile.ContactPhone = request.ContactPhone?.Trim();
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(profile.Id, "Client profili kaydedildi.");
    }
}