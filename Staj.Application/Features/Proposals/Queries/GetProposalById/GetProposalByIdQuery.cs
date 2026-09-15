// Staj.Application/Features/Proposals/Queries/GetProposalById/GetProposalByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;

namespace Staj.Application.Features.Proposals.Queries.GetProposalById;

// Teklif detay sorgusu
public record GetProposalByIdQuery(Guid Id) : IRequest<Result<ProposalDto>>;