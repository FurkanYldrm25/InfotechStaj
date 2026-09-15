// Staj.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Auth.Commands.Login;

namespace Staj.Application.Features.Auth.Commands.RefreshToken;

// Refresh token ile yeni access token alma komutu
public record RefreshTokenCommand(string AccessToken, string RefreshToken)
    : IRequest<Result<LoginResponse>>;