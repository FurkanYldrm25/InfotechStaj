// Staj.Application/Features/Users/Commands/UpdateProfileImage/UpdateProfileImageCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Users.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandHandler
    : IRequestHandler<UpdateProfileImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProfileImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        UpdateProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null) return Result.Fail("Kullanıcı bulunamadı.");

        user.ProfileImageUrl = request.ImageUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Profil fotoğrafı güncellendi.");
    }
}