// Staj.Application/Features/Categories/Commands/RemoveSkill/RemoveSkillFromCategoryCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.RemoveSkill;

// Skill kaldırma işleyicisi
public class RemoveSkillFromCategoryCommandHandler
    : IRequestHandler<RemoveSkillFromCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveSkillFromCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        RemoveSkillFromCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var link = await _context.CategorySkills
            .FirstOrDefaultAsync(cs => cs.CategoryId == request.CategoryId
                                    && cs.SkillId == request.SkillId, cancellationToken);

        if (link is null)
            return Result.Fail("Bu bağlantı bulunamadı.");

        _context.CategorySkills.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok("Skill kategoriden kaldırıldı.");
    }
}