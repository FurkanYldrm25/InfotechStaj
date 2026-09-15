// Staj.Application/Features/Reviews/Queries/GetReviewsForUser/GetReviewsForUserQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetReviewsForUser;

// Bir kullanıcının aldığı değerlendirmeleri getirir
public class GetReviewsForUserQueryHandler
    : IRequestHandler<GetReviewsForUserQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetReviewsForUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetReviewsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Reviews
            .AsNoTracking()
            .Include(r => r.Author)
            .Include(r => r.Target)
            .Include(r => r.Proposal).ThenInclude(p => p.JobPost)
            .Where(r => r.TargetUserId == request.UserId && r.IsVisible);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto(
                r.Id,
                r.ProposalId,
                r.Proposal.JobPostId,
                r.Proposal.JobPost != null ? r.Proposal.JobPost.Title : null,
                r.AuthorUserId,
                r.Author.FirstName,
                r.Author.LastName,
                r.Author.ProfileImageUrl,
                r.TargetUserId,
                r.Target.FirstName,
                r.Target.LastName,
                r.Rating,
                r.Comment,
                r.IsVisible,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ReviewDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<ReviewDto>>.Ok(result);
    }
}