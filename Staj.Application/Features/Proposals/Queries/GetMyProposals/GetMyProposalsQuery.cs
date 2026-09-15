// Staj.Application/Features/Proposals/Queries/GetMyProposals/GetMyProposalsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Queries.GetMyProposals;

// Kullanıcının rolüne göre yön: Sent (gönderilen) veya Received (alınan)
public enum ProposalDirection
{
    Sent = 1,
    Received = 2
}

// Kullanıcının kendi tekliflerini getiren sorgu
public record GetMyProposalsQuery(
    ProposalDirection Direction,
    ProposalKind? Kind,
    ProposalStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ProposalListItemDto>>>;