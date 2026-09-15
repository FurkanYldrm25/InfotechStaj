// Staj.Application/Features/Reviews/Queries/GetMyGivenReviews/GetMyGivenReviewsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetMyGivenReviews;

// Kullanıcının yazdığı değerlendirmeler
public record GetMyGivenReviewsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReviewDto>>>;