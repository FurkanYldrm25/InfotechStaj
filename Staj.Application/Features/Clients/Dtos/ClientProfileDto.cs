// Staj.Application/Features/Clients/Dtos/ClientProfileDto.cs
using Staj.Domain.Enums;

namespace Staj.Application.Features.Clients.Dtos;

// Client profil DTO'su
public record ClientProfileDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? ProfileImageUrl,
    string? CompanyName,
    string? Industry,
    string? About,
    string? WebsiteUrl,
    string? Country,
    string? City,
    ContactPreference ContactPreference,
    string? ContactPhone
);