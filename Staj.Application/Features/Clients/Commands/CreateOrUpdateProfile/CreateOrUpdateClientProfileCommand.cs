// Staj.Application/Features/Clients/Commands/CreateOrUpdateProfile/CreateOrUpdateClientProfileCommand.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Clients.Commands.CreateOrUpdateProfile;

// Client profili upsert
public record CreateOrUpdateClientProfileCommand(
    string? CompanyName,
    string? Industry,
    string? About,
    string? WebsiteUrl,
    string? Country,
    string? City,
    ContactPreference ContactPreference,
    string? ContactPhone
) : IRequest<Result<Guid>>;