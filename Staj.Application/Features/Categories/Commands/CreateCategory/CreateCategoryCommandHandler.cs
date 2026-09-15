// Staj.Application/Features/Categories/Commands/CreateCategory/CreateCategoryCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Categories.Commands.CreateCategory;

// Kategori oluşturma işleyicisi
public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var slug = SlugHelper.Generate(request.Name);

        var exists = await _context.Categories
            .AnyAsync(c => c.Slug == slug, cancellationToken);
        if (exists)
            return Result<Guid>.Fail("Bu isimde bir kategori zaten mevcut.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Slug = slug,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(category.Id, "Kategori oluşturuldu.");
    }
}