// Staj.Application/Features/JobPosts/Commands/UpdateJobPost/UpdateJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.UpdateJobPost;

// İş ilanı güncelleme işleyicisi
public class UpdateJobPostCommandHandler
    : IRequestHandler<UpdateJobPostCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        UpdateJobPostCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var jobPost = await _context.JobPosts
            .Include(j => j.ClientProfile)
            .Include(j => j.JobPostSkills)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobPost is null)
            return Result.Fail("İlan bulunamadı.");

        // Sadece kendi ilanını güncelleyebilir
        if (jobPost.ClientProfile.UserId != userId.Value)
            return Result.Fail("Bu ilanı güncelleme yetkiniz yok.");

        // Kapalı/iptal ilan güncellenemez
        if (jobPost.Status is JobPostStatus.Closed or JobPostStatus.Cancelled)
            return Result.Fail("Kapalı veya iptal edilmiş ilan güncellenemez.");

        // Kategori doğrula
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);
        if (!categoryExists)
            return Result.Fail("Geçerli bir kategori seçin.");

        jobPost.CategoryId = request.CategoryId;
        jobPost.Title = request.Title.Trim();
        jobPost.Description = request.Description.Trim();
        jobPost.BudgetMin = request.BudgetMin;
        jobPost.BudgetMax = request.BudgetMax;
        jobPost.Currency = request.Currency?.Trim();
        jobPost.WorkMode = request.WorkMode;
        jobPost.DurationDays = request.DurationDays;
        jobPost.Country = request.Country?.Trim();
        jobPost.City = request.City?.Trim();
        jobPost.UpdatedAt = DateTime.UtcNow;

        // Skill listesini yeniden yaz
        _context.JobPostSkills.RemoveRange(jobPost.JobPostSkills);

        if (request.SkillIds is { Count: > 0 })
        {
            var requestedIds = request.SkillIds.Distinct().ToList();
            var validSkillIds = await _context.Skills
                .Where(s => requestedIds.Contains(s.Id) && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            foreach (var skillId in validSkillIds)
            {
                _context.JobPostSkills.Add(new JobPostSkill
                {
                    JobPostId = jobPost.Id,
                    SkillId = skillId
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("İlan güncellendi.");
    }
}