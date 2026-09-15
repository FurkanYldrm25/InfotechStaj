// Staj.Application/Features/Auth/Commands/Login/LoginCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Auth.Commands.Login;

// Giriş komutu
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

// Giriş sonucu
public record LoginResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);