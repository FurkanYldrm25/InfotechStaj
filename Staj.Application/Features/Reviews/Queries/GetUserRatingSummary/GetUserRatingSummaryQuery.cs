// Staj.Application/Features/Reviews/Queries/GetUserRatingSummary/GetUserRatingSummaryQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Reviews.Dtos;

namespace Staj.Application.Features.Reviews.Queries.GetUserRatingSummary;

// Kullanıcının ortalama puan ve toplam yorum sayısını getirir
public record GetUserRatingSummaryQuery(Guid UserId)
    : IRequest<Result<UserRatingSummaryDto>>;