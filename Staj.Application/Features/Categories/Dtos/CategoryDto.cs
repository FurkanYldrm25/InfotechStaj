// Staj.Application/Features/Categories/Dtos/CategoryDto.cs
namespace Staj.Application.Features.Categories.Dtos;

// Kategori veri transfer nesnesi
public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    bool IsActive,
    int SkillCount,
    DateTime CreatedAt
);

// Kategori detayı (skill listesi ile)
public record CategoryDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    bool IsActive,
    DateTime CreatedAt,
    List<SkillSummaryDto> Skills
);

// Kategori altındaki skill özeti
public record SkillSummaryDto(Guid Id, string Name, string Slug);