// Staj.Application/Features/Reviews/Queries/GetMyGivenReviews/GetMyGivenReviewsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetMyGivenReviews;

// Kullanıcının kendi yazdığı değerlendirmeler
public class GetMyGivenReviewsQueryHandler
    : IRequestHandler<GetMyGivenReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyGivenReviewsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetMyGivenReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<ReviewDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Reviews
            .AsNoTracking()
            .Include(r => r.Author)
            .Include(r => r.Target)
            .Include(r => r.Proposal).ThenInclude(p => p.JobPost)
            .Where(r => r.AuthorUserId == userId.Value);

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