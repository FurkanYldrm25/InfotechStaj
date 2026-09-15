// Staj.Application/Features/Auth/Commands/Register/RegisterCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Auth.Commands.Register;

// Yeni kullanıcı kayıt komutu
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role
) : IRequest<Result<RegisterResponse>>;

// Kayıt sonucu
public record RegisterResponse(Guid UserId, string Email, string Role);