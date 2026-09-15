// Staj.Application/Features/Proposals/Dtos/ProposalDto.cs
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Dtos;

// Teklif detay DTO'su
public record ProposalDto(
    Guid Id,
    ProposalKind Kind,
    ProposalStatus Status,
    Guid? JobPostId,
    string? JobPostTitle,
    Guid FreelancerProfileId,
    Guid FreelancerUserId,
    string FreelancerFirstName,
    string FreelancerLastName,
    string? FreelancerTitle,
    Guid ClientProfileId,
    Guid ClientUserId,
    string? ClientCompanyName,
    string ClientFirstName,
    string ClientLastName,
    string CoverMessage,
    decimal? ProposedRate,
    int? ProposedDurationDays,
    string? Currency,
    DateTime? RespondedAt,
    string? ResponseNote,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// Teklif liste DTO'su (özet)
public record ProposalListItemDto(
    Guid Id,
    ProposalKind Kind,
    ProposalStatus Status,
    Guid? JobPostId,
    string? JobPostTitle,
    Guid FreelancerProfileId,
    string FreelancerFirstName,
    string FreelancerLastName,
    string? FreelancerTitle,
    Guid ClientProfileId,
    string? ClientCompanyName,
    string ClientFirstName,
    string ClientLastName,
    decimal? ProposedRate,
    int? ProposedDurationDays,
    string? Currency,
    DateTime CreatedAt
);