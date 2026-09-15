// Staj.Application/Features/Clients/Queries/GetMyClientProfile/GetMyClientProfileQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Clients.Dtos;

namespace Staj.Application.Features.Clients.Queries.GetMyClientProfile;

public record GetMyClientProfileQuery() : IRequest<Result<ClientProfileDto>>;