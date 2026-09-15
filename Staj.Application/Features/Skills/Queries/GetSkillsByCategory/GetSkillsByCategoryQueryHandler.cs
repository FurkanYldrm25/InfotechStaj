// Staj.Application/Features/Skills/Queries/GetSkillsByCategory/GetSkillsByCategoryQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetSkillsByCategory;

// Kategoriye göre skill listeleme işleyicisi
public class GetSkillsByCategoryQueryHandler
    : IRequestHandler<GetSkillsByCategoryQuery, Result<List<SkillDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSkillsByCategoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SkillDto>>> Handle(
        GetSkillsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result<List<SkillDto>>.Fail("Kategori bulunamadı.");

        var data = await _context.CategorySkills
            .AsNoTracking()
            .Where(cs => cs.CategoryId == request.CategoryId && cs.Skill.IsActive)
            .OrderBy(cs => cs.Skill.Name)
            .Select(cs => new SkillDto(
                cs.Skill.Id,
                cs.Skill.Name,
                cs.Skill.Description,
                cs.Skill.Slug,
                cs.Skill.IsActive,
                cs.Skill.CategorySkills.Count,
                cs.Skill.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<SkillDto>>.Ok(data);
    }
}