// Staj.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Common.Settings;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Auth.Commands.Login;

// Giriş komutu işleyicisi
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted)
            return Result<LoginResponse>.Fail("Email veya şifre hatalı.");

        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return Result<LoginResponse>.Fail("Email veya şifre hatalı.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = await _tokenService.GenerateTokensAsync(user, roles);

        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        user.LastLoginAt = DateTime.UtcNow;
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

        return Result<LoginResponse>.Ok(response, "Giriş başarılı.");
    }
}