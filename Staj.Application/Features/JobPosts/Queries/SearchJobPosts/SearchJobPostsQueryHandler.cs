// Staj.Application/Features/JobPosts/Queries/SearchJobPosts/SearchJobPostsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Queries.SearchJobPosts;

// İş ilanı arama işleyicisi
public class SearchJobPostsQueryHandler
    : IRequestHandler<SearchJobPostsQuery, Result<PagedResult<JobPostListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchJobPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<JobPostListItemDto>>> Handle(
        SearchJobPostsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.JobPosts
            .AsNoTracking()
            .Include(j => j.Category)
            .Include(j => j.ClientProfile)
            .Include(j => j.JobPostSkills).ThenInclude(js => js.Skill)
            .Where(j => j.Status == JobPostStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            query = query.Where(j =>
                EF.Functions.Like(j.Title, $"%{kw}%") ||
                EF.Functions.Like(j.Description, $"%{kw}%"));
        }

        if (request.CategoryId.HasValue)
        {
            var catId = request.CategoryId.Value;
            query = query.Where(j => j.CategoryId == catId);
        }

        if (request.SkillIds is { Count: > 0 })
        {
            var ids = request.SkillIds;
            query = query.Where(j => j.JobPostSkills.Any(js => ids.Contains(js.SkillId)));
        }

        if (request.MinBudget.HasValue)
        {
            var min = request.MinBudget.Value;
            // İlanın üst sınırı, aranan minimumdan büyük ya da eşit olmalı
            query = query.Where(j => j.BudgetMax == null || j.BudgetMax >= min);
        }

        if (request.MaxBudget.HasValue)
        {
            var max = request.MaxBudget.Value;
            // İlanın alt sınırı, aranan maksimumdan küçük ya da eşit olmalı
            query = query.Where(j => j.BudgetMin == null || j.BudgetMin <= max);
        }

        if (request.WorkMode.HasValue)
            query = query.Where(j => j.WorkMode == request.WorkMode.Value);

        if (!string.IsNullOrWhiteSpace(request.Country))
            query = query.Where(j => j.Country == request.Country);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(j => j.City == request.City);

        if (request.PublishedAfter.HasValue)
        {
            var after = request.PublishedAfter.Value;
            query = query.Where(j => j.PublishedAt != null && j.PublishedAt >= after);
        }

        // Sıralama
        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "budget_desc" => query.OrderByDescending(j => j.BudgetMax ?? j.BudgetMin ?? 0),
            "budget_asc" => query.OrderBy(j => j.BudgetMin ?? j.BudgetMax ?? 0),
            "oldest" => query.OrderBy(j => j.PublishedAt ?? j.CreatedAt),
            _ => query.OrderByDescending(j => j.PublishedAt ?? j.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);

        var items = await query
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