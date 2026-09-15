// Staj.Application/Features/Freelancers/Queries/SearchFreelancers/SearchFreelancersQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;

namespace Staj.Application.Features.Freelancers.Queries.SearchFreelancers;

// Freelancer arama işleyicisi
public class SearchFreelancersQueryHandler
    : IRequestHandler<SearchFreelancersQuery, Result<PagedResult<FreelancerListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchFreelancersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<FreelancerListItemDto>>> Handle(
        SearchFreelancersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.FreelancerProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.FreelancerSkills).ThenInclude(fs => fs.Skill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            query = query.Where(p =>
                (p.Title != null && EF.Functions.Like(p.Title, $"%{kw}%")) ||
                (p.Bio != null && EF.Functions.Like(p.Bio, $"%{kw}%")) ||
                EF.Functions.Like(p.User.FirstName, $"%{kw}%") ||
                EF.Functions.Like(p.User.LastName, $"%{kw}%"));
        }

        if (request.SkillIds is { Count: > 0 })
        {
            var ids = request.SkillIds;
            query = query.Where(p => p.FreelancerSkills.Any(fs => ids.Contains(fs.SkillId)));
        }

        if (request.CategoryId.HasValue)
        {
            var catId = request.CategoryId.Value;
            query = query.Where(p =>
                p.FreelancerSkills.Any(fs =>
                    fs.Skill.CategorySkills.Any(cs => cs.CategoryId == catId)));
        }

        if (request.MinHourlyRate.HasValue)
        {
            var min = request.MinHourlyRate.Value;
            query = query.Where(p => p.HourlyRateMax == null || p.HourlyRateMax >= min);
        }

        if (request.MaxHourlyRate.HasValue)
        {
            var max = request.MaxHourlyRate.Value;
            query = query.Where(p => p.HourlyRateMin == null || p.HourlyRateMin <= max);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
            query = query.Where(p => p.Country == request.Country);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(p => p.City == request.City);

        if (request.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new FreelancerListItemDto(
                p.Id,
                p.UserId,
                p.User.FirstName,
                p.User.LastName,
                p.User.ProfileImageUrl,
                p.Title,
                p.ExperienceYears,
                p.HourlyRateMin,
                p.HourlyRateMax,
                p.Currency,
                p.Country,
                p.City,
                p.IsAvailable,
                p.FreelancerSkills
                    .OrderBy(fs => fs.Skill.Name)
                    .Select(fs => fs.Skill.Name)
                    .ToList()))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<FreelancerListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<FreelancerListItemDto>>.Ok(result);
    }
}