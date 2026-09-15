// Staj.Application/Features/Reviews/Queries/GetUserRatingSummary/GetUserRatingSummaryQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetUserRatingSummary;

// Kullanıcının ortalama puan ve yorum sayısı
public class GetUserRatingSummaryQueryHandler
    : IRequestHandler<GetUserRatingSummaryQuery, Result<UserRatingSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserRatingSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserRatingSummaryDto>> Handle(
        GetUserRatingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var visibleReviews = _context.Reviews
            .AsNoTracking()
            .Where(r => r.TargetUserId == request.UserId && r.IsVisible);

        var count = await visibleReviews.CountAsync(cancellationToken);

        double average = 0;
        if (count > 0)
            average = await visibleReviews.AverageAsync(r => (double)r.Rating, cancellationToken);

        var dto = new UserRatingSummaryDto(
            request.UserId,
            Math.Round(average, 2),
            count);

        return Result<UserRatingSummaryDto>.Ok(dto);
    }
}