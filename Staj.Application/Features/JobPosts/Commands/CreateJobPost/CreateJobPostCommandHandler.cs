// Staj.Application/Features/JobPosts/Commands/CreateJobPost/CreateJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.CreateJobPost;

// İş ilanı oluşturma işleyicisi
public class CreateJobPostCommandHandler
    : IRequestHandler<CreateJobPostCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CreateJobPostCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        // İlan açan kullanıcının Client profili olmalı
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);

        if (clientProfile is null)
            return Result<Guid>.Fail("Önce Client profilinizi oluşturun.");

        // Kategori doğrula
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);
        if (!categoryExists)
            return Result<Guid>.Fail("Geçerli bir kategori seçin.");

        // Slug üret ve çakışmayı sayaçla çöz
        var baseSlug = SlugHelper.Generate(request.Title);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "ilan";

        var slug = baseSlug;
        var counter = 1;
        while (await _context.JobPosts.AnyAsync(j => j.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        var jobPost = new JobPost
        {
            ClientProfileId = clientProfile.Id,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            BudgetMin = request.BudgetMin,
            BudgetMax = request.BudgetMax,
            Currency = request.Currency?.Trim(),
            WorkMode = request.WorkMode,
            DurationDays = request.DurationDays,
            Country = request.Country?.Trim(),
            City = request.City?.Trim(),
            Status = JobPostStatus.Draft
        };

        _context.JobPosts.Add(jobPost);

        // Skill bağlarını ekle (sadece geçerli olanları)
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
        return Result<Guid>.Ok(jobPost.Id, "İş ilanı oluşturuldu.");
    }
}