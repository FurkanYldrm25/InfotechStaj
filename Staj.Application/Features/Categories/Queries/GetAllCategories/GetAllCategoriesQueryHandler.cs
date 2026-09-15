// Staj.Application/Features/Categories/Queries/GetAllCategories/GetAllCategoriesQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Categories.Dtos;

namespace Staj.Application.Features.Categories.Queries.GetAllCategories;

// Tüm kategorileri getiren sorgu işleyicisi
public class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsNoTracking();

        if (request.OnlyActive)
            query = query.Where(c => c.IsActive);

        var data = await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Slug,
                c.IsActive,
                c.CategorySkills.Count,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<CategoryDto>>.Ok(data);
    }
}