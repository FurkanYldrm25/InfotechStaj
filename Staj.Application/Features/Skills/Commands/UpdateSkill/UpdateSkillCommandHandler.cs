// Staj.Application/Features/Skills/Commands/UpdateSkill/UpdateSkillCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Skills.Commands.UpdateSkill;

// Skill güncelleme işleyicisi
public class UpdateSkillCommandHandler
    : IRequestHandler<UpdateSkillCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        UpdateSkillCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill is null)
            return Result.Fail("Skill bulunamadı.");

        var newSlug = SlugHelper.Generate(request.Name);

        var slugTaken = await _context.Skills
            .AnyAsync(s => s.Slug == newSlug && s.Id != request.Id, cancellationToken);
        if (slugTaken)
            return Result.Fail("Bu isimde başka bir skill zaten mevcut.");

        skill.Name = request.Name.Trim();
        skill.Description = request.Description?.Trim();
        skill.Slug = newSlug;
        skill.IsActive = request.IsActive;
        skill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok("Skill güncellendi.");
    }
}