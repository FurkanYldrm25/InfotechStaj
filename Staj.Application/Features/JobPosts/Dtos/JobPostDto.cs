// Staj.Application/Features/JobPosts/Dtos/JobPostDto.cs
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Dtos;

// İş ilanı detay DTO'su
public record JobPostDto(
    Guid Id,
    Guid ClientProfileId,
    Guid ClientUserId,
    string? ClientCompanyName,
    string ClientFirstName,
    string ClientLastName,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Description,
    decimal? BudgetMin,
    decimal? BudgetMax,
    string? Currency,
    WorkMode WorkMode,
    int? DurationDays,
    string? Country,
    string? City,
    JobPostStatus Status,
    DateTime? PublishedAt,
    DateTime? ClosedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<JobPostSkillDto> Skills
);

// İş ilanı liste DTO'su
public record JobPostListItemDto(
    Guid Id,
    Guid ClientProfileId,
    string? ClientCompanyName,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    decimal? BudgetMin,
    decimal? BudgetMax,
    string? Currency,
    WorkMode WorkMode,
    int? DurationDays,
    string? Country,
    string? City,
    JobPostStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    List<string> Skills
);

// JobPost içindeki skill özet DTO'su
public record JobPostSkillDto(Guid SkillId, string Name, string Slug);