// Staj.Application/Features/Skills/Commands/DeleteSkill/DeleteSkillCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Skills.Commands.DeleteSkill;

// Skill silme işleyicisi
public class DeleteSkillCommandHandler
    : IRequestHandler<DeleteSkillCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteSkillCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill is null)
            return Result.Fail("Skill bulunamadı.");

        skill.IsDeleted = true;
        skill.DeletedAt = DateTime.UtcNow;
        skill.IsActive = false;
        skill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok("Skill silindi.");
    }
}