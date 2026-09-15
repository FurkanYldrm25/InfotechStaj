// Staj.Application/Features/JobPosts/Queries/GetJobPostById/GetJobPostByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Queries.GetJobPostById;

// İş ilanı detay işleyicisi
public class GetJobPostByIdQueryHandler
    : IRequestHandler<GetJobPostByIdQuery, Result<JobPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetJobPostByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<JobPostDto>> Handle(
        GetJobPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        var jobPost = await _context.JobPosts
            .AsNoTracking()
            .Include(j => j.Category)
            .Include(j => j.ClientProfile).ThenInclude(c => c.User)
            .Include(j => j.JobPostSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobPost is null)
            return Result<JobPostDto>.Fail("İlan bulunamadı.");

        // Yayında değilse: yalnızca sahibi görebilir
        if (jobPost.Status != JobPostStatus.Published)
        {
            var currentUserId = _currentUser.UserId;
            if (currentUserId is null || jobPost.ClientProfile.UserId != currentUserId.Value)
                return Result<JobPostDto>.Fail("İlan bulunamadı.");
        }

        var dto = new JobPostDto(
            jobPost.Id,
            jobPost.ClientProfileId,
            jobPost.ClientProfile.UserId,
            jobPost.ClientProfile.CompanyName,
            jobPost.ClientProfile.User.FirstName,
            jobPost.ClientProfile.User.LastName,
            jobPost.CategoryId,
            jobPost.Category.Name,
            jobPost.Title,
            jobPost.Slug,
            jobPost.Description,
            jobPost.BudgetMin,
            jobPost.BudgetMax,
            jobPost.Currency,
            jobPost.WorkMode,
            jobPost.DurationDays,
            jobPost.Country,
            jobPost.City,
            jobPost.Status,
            jobPost.PublishedAt,
            jobPost.ClosedAt,
            jobPost.CreatedAt,
            jobPost.UpdatedAt,
            jobPost.JobPostSkills
                .OrderBy(js => js.Skill.Name)
                .Select(js => new JobPostSkillDto(js.SkillId, js.Skill.Name, js.Skill.Slug))
                .ToList());

        return Result<JobPostDto>.Ok(dto);
    }
}