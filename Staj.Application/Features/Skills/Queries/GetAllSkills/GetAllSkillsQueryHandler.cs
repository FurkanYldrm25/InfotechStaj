// Staj.Application/Features/Skills/Queries/GetAllSkills/GetAllSkillsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetAllSkills;

// Tüm skill'leri getiren sorgu işleyicisi
public class GetAllSkillsQueryHandler
    : IRequestHandler<GetAllSkillsQuery, Result<List<SkillDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSkillsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SkillDto>>> Handle(
        GetAllSkillsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Skills.AsNoTracking();

        if (request.OnlyActive)
            query = query.Where(s => s.IsActive);

        var data = await query
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto(
                s.Id,
                s.Name,
                s.Description,
                s.Slug,
                s.IsActive,
                s.CategorySkills.Count,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<SkillDto>>.Ok(data);
    }
}