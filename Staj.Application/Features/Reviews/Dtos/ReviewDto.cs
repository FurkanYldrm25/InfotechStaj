// Staj.Application/Features/Reviews/Dtos/ReviewDto.cs
namespace Staj.Application.Features.Reviews.Dtos;

// Tekil review detay/liste DTO'su
public record ReviewDto(
    Guid Id,
    Guid ProposalId,
    Guid? JobPostId,
    string? JobPostTitle,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName,
    string? AuthorProfileImageUrl,
    Guid TargetUserId,
    string TargetFirstName,
    string TargetLastName,
    int Rating,
    string Comment,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// Kullanıcı puan özeti
public record UserRatingSummaryDto(
    Guid UserId,
    double AverageRating,
    int ReviewCount
);