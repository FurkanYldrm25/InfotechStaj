// Staj.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Common.Settings;
using Staj.Application.Features.Auth.Commands.Login;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Auth.Commands.RefreshToken;

// Refresh token işleyicisi
public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var user = _userManager.Users.FirstOrDefault(u =>
            u.RefreshToken == request.RefreshToken && !u.IsDeleted);

        if (user is null)
            return Result<LoginResponse>.Fail("Geçersiz refresh token.");

        if (user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return Result<LoginResponse>.Fail("Refresh token süresi dolmuş.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = await _tokenService.GenerateTokensAsync(user, roles);

        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        var response = new LoginResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresAt);

        return Result<LoginResponse>.Ok(response, "Token yenilendi.");
    }
}