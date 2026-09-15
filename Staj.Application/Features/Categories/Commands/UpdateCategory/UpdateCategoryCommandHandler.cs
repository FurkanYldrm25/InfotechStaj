// Staj.Application/Features/Categories/Commands/UpdateCategory/UpdateCategoryCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.UpdateCategory;

// Kategori güncelleme işleyicisi
public class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Fail("Kategori bulunamadı.");

        var newSlug = SlugHelper.Generate(request.Name);

        var slugTaken = await _context.Categories
            .AnyAsync(c => c.Slug == newSlug && c.Id != request.Id, cancellationToken);
        if (slugTaken)
            return Result.Fail("Bu isimde başka bir kategori zaten mevcut.");

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.Slug = newSlug;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok("Kategori güncellendi.");
    }
}