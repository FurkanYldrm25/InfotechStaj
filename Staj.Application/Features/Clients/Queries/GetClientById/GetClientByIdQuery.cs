// Staj.Application/Features/Clients/Queries/GetClientById/GetClientByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Clients.Dtos;

namespace Staj.Application.Features.Clients.Queries.GetClientById;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientProfileDto>>;