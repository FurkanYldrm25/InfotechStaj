// Staj.Application/Features/Proposals/Queries/GetProposalsForJobPost/GetProposalsForJobPostQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Queries.GetProposalsForJobPost;

// Belirli bir ilana gelen başvuruları getiren sorgu (yalnızca ilan sahibi Client)
public record GetProposalsForJobPostQuery(
    Guid JobPostId,
    ProposalStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ProposalListItemDto>>>;