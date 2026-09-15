// Staj.Application/Features/Reviews/Queries/GetMyReceivedReviews/GetMyReceivedReviewsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetMyReceivedReviews;

// Kullanıcıya yapılan değerlendirmeler (gizli olanlar dahil, kendi görebilir)
public record GetMyReceivedReviewsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReviewDto>>>;