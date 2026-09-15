// Staj.Application/Features/Categories/Commands/AssignSkill/AssignSkillToCategoryCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Categories.Commands.AssignSkill;

// Skill atama işleyicisi
public class AssignSkillToCategoryCommandHandler
    : IRequestHandler<AssignSkillToCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignSkillToCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        AssignSkillToCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Fail("Kategori bulunamadı.");

        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == request.SkillId, cancellationToken);
        if (!skillExists)
            return Result.Fail("Skill bulunamadı.");

        var alreadyLinked = await _context.CategorySkills
            .AnyAsync(cs => cs.CategoryId == request.CategoryId
                         && cs.SkillId == request.SkillId, cancellationToken);
        if (alreadyLinked)
            return Result.Fail("Bu skill zaten bu kategoriye bağlı.");

        _context.CategorySkills.Add(new CategorySkill
        {
            CategoryId = request.CategoryId,
            SkillId = request.SkillId
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok("Skill kategoriye eklendi.");
    }
}