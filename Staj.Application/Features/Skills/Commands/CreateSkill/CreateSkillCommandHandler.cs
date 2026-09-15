// Staj.Application/Features/Skills/Commands/CreateSkill/CreateSkillCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Skills.Commands.CreateSkill;

// Skill oluşturma işleyicisi
public class CreateSkillCommandHandler
    : IRequestHandler<CreateSkillCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateSkillCommand request,
        CancellationToken cancellationToken)
    {
        var slug = SlugHelper.Generate(request.Name);

        var exists = await _context.Skills
            .AnyAsync(s => s.Slug == slug, cancellationToken);
        if (exists)
            return Result<Guid>.Fail("Bu isimde bir skill zaten mevcut.");

        var skill = new Skill
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Slug = slug,
            IsActive = true
        };

        if (request.CategoryIds is not null && request.CategoryIds.Count > 0)
        {
            var validCategoryIds = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var categoryId in validCategoryIds)
            {
                skill.CategorySkills.Add(new CategorySkill
                {
                    CategoryId = categoryId,
                    SkillId = skill.Id
                });
            }
        }

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(skill.Id, "Skill oluşturuldu.");
    }
}