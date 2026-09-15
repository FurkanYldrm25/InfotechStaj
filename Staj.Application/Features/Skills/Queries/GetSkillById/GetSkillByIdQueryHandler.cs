// Staj.Application/Features/Skills/Queries/GetSkillById/GetSkillByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetSkillById;

// Skill detay işleyicisi
public class GetSkillByIdQueryHandler
    : IRequestHandler<GetSkillByIdQuery, Result<SkillDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSkillByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillDetailDto>> Handle(
        GetSkillByIdQuery request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .AsNoTracking()
            .Include(s => s.CategorySkills)
                .ThenInclude(cs => cs.Category)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill is null)
            return Result<SkillDetailDto>.Fail("Skill bulunamadı.");

        var dto = new SkillDetailDto(
            skill.Id,
            skill.Name,
            skill.Description,
            skill.Slug,
            skill.IsActive,
            skill.CreatedAt,
            skill.CategorySkills
                .Where(cs => cs.Category.IsActive)
                .OrderBy(cs => cs.Category.Name)
                .Select(cs => new CategorySummaryDto(cs.Category.Id, cs.Category.Name, cs.Category.Slug))
                .ToList());

        return Result<SkillDetailDto>.Ok(dto);
    }
}