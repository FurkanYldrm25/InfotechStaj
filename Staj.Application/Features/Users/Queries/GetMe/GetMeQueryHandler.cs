// Staj.Application/Features/Users/Queries/GetMe/GetMeQueryHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Users.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetMeQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<MeDto>.Fail("Yetkisiz erişim.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null) return Result<MeDto>.Fail("Kullanıcı bulunamadı.");

        var roles = await _userManager.GetRolesAsync(user);

        var hasFreelancer = await _context.FreelancerProfiles
            .AnyAsync(p => p.UserId == user.Id, cancellationToken);
        var hasClient = await _context.ClientProfiles
            .AnyAsync(p => p.UserId == user.Id, cancellationToken);

        return Result<MeDto>.Ok(new MeDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.ProfileImageUrl,
            roles.ToList(),
            hasFreelancer,
            hasClient));
    }
}