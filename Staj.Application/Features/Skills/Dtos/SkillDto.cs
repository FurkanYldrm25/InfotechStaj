// Staj.Application/Features/Skills/Dtos/SkillDto.cs
namespace Staj.Application.Features.Skills.Dtos;

// Skill veri transfer nesnesi
public record SkillDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    bool IsActive,
    int CategoryCount,
    DateTime CreatedAt
);

// Skill detayı (kategori listesi ile)
public record SkillDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    bool IsActive,
    DateTime CreatedAt,
    List<CategorySummaryDto> Categories
);

// Skill'in bağlı olduğu kategori özeti
public record CategorySummaryDto(Guid Id, string Name, string Slug);