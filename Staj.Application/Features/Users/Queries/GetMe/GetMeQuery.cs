// Staj.Application/Features/Users/Queries/GetMe/GetMeQuery.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Users.Queries.GetMe;

// Aktif kullanıcı özeti
public record GetMeQuery() : IRequest<Result<MeDto>>;

public record MeDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? ProfileImageUrl,
    List<string> Roles,
    bool HasFreelancerProfile,
    bool HasClientProfile
);