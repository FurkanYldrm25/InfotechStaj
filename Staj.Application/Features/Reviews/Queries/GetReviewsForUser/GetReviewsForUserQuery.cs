// Staj.Application/Features/Reviews/Queries/GetReviewsForUser/GetReviewsForUserQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetReviewsForUser;

// Bir kullanıcının aldığı değerlendirmeleri getirir (public)
public record GetReviewsForUserQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReviewDto>>>;