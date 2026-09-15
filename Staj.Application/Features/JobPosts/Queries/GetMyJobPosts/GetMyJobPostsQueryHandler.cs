// Staj.Application/Features/JobPosts/Queries/GetMyJobPosts/GetMyJobPostsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;

namespace Staj.Application.Features.JobPosts.Queries.GetMyJobPosts;

// Client'ın kendi ilanlarını getiren sorgu işleyicisi
public class GetMyJobPostsQueryHandler
    : IRequestHandler<GetMyJobPostsQuery, Result<PagedResult<JobPostListItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyJobPostsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<JobPostListItemDto>>> Handle(
        GetMyJobPostsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<JobPostListItemDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var clientProfile = await _context.ClientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);

        if (clientProfile is null)
        {
            // Profil yoksa boş liste dön
            return Result<PagedResult<JobPostListItemDto>>.Ok(new PagedResult<JobPostListItemDto>
            {
                Items = new List<JobPostListItemDto>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            });
        }

        var query = _context.JobPosts
            .AsNoTracking()
            .Include(j => j.Category)
            .Include(j => j.ClientProfile)
            .Include(j => j.JobPostSkills).ThenInclude(js => js.Skill)
            .Where(j => j.ClientProfileId == clientProfile.Id);

        if (request.Status.HasValue)
            query = query.Where(j => j.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobPostListItemDto(
                j.Id,
                j.ClientProfileId,
                j.ClientProfile.CompanyName,
                j.CategoryId,
                j.Category.Name,
                j.Title,
                j.Slug,
                j.BudgetMin,
                j.BudgetMax,
                j.Currency,
                j.WorkMode,
                j.DurationDays,
                j.Country,
                j.City,
                j.Status,
                j.PublishedAt,
                j.CreatedAt,
                j.JobPostSkills
                    .OrderBy(js => js.Skill.Name)
                    .Select(js => js.Skill.Name)
                    .ToList()))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<JobPostListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<JobPostListItemDto>>.Ok(result);
    }
}