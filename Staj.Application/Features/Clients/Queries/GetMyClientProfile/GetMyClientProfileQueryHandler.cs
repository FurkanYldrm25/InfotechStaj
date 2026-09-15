// Staj.Application/Features/Clients/Queries/GetMyClientProfile/GetMyClientProfileQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Clients.Dtos;

namespace Staj.Application.Features.Clients.Queries.GetMyClientProfile;

public class GetMyClientProfileQueryHandler
    : IRequestHandler<GetMyClientProfileQuery, Result<ClientProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyClientProfileQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ClientProfileDto>> Handle(
        GetMyClientProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<ClientProfileDto>.Fail("Yetkisiz erişim.");

        var profile = await _context.ClientProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
            return Result<ClientProfileDto>.Fail("Profil bulunamadı.");

        var dto = new ClientProfileDto(
            profile.Id, profile.UserId,
            profile.User.FirstName, profile.User.LastName, profile.User.Email ?? string.Empty,
            profile.User.ProfileImageUrl,
            profile.CompanyName, profile.Industry, profile.About, profile.WebsiteUrl,
            profile.Country, profile.City,
            profile.ContactPreference, profile.ContactPhone);

        return Result<ClientProfileDto>.Ok(dto);
    }
}