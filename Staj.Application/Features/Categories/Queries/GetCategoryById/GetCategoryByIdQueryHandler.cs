// Staj.Application/Features/Categories/Queries/GetCategoryById/GetCategoryByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Categories.Dtos;

namespace Staj.Application.Features.Categories.Queries.GetCategoryById;

// Kategori detayı işleyicisi
public class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryDetailDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.CategorySkills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result<CategoryDetailDto>.Fail("Kategori bulunamadı.");

        var dto = new CategoryDetailDto(
            category.Id,
            category.Name,
            category.Description,
            category.Slug,
            category.IsActive,
            category.CreatedAt,
            category.CategorySkills
                .Where(cs => cs.Skill.IsActive)
                .OrderBy(cs => cs.Skill.Name)
                .Select(cs => new SkillSummaryDto(cs.Skill.Id, cs.Skill.Name, cs.Skill.Slug))
                .ToList());

        return Result<CategoryDetailDto>.Ok(dto);
    }
}