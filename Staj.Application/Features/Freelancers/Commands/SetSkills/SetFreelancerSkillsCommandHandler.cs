// Staj.Application/Features/Freelancers/Commands/SetSkills/SetFreelancerSkillsCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Freelancers.Commands.SetSkills;

// Skill listesini yeniden yazan işleyici
public class SetFreelancerSkillsCommandHandler
    : IRequestHandler<SetFreelancerSkillsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetFreelancerSkillsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        SetFreelancerSkillsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var profile = await _context.FreelancerProfiles
            .Include(p => p.FreelancerSkills)
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
            return Result.Fail("Önce freelancer profilinizi oluşturun.");

        // Girilen skill'lerin sistemde bulunup bulunmadığını doğrula
        var requestedIds = request.Skills.Select(s => s.SkillId).Distinct().ToList();
        var validSkillIds = await _context.Skills
            .Where(s => requestedIds.Contains(s.Id) && s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        // Mevcut skill bağlarını temizle
        _context.FreelancerSkills.RemoveRange(profile.FreelancerSkills);

        foreach (var input in request.Skills.Where(s => validSkillIds.Contains(s.SkillId)))
        {
            _context.FreelancerSkills.Add(new FreelancerSkill
            {
                FreelancerProfileId = profile.Id,
                SkillId = input.SkillId,
                ProficiencyLevel = input.ProficiencyLevel
            });
        }

        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Skill listesi güncellendi.");
    }
}