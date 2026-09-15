// Staj.Application/Features/Categories/Commands/UpdateCategory/UpdateCategoryCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.UpdateCategory;

// Kategori güncelleme komutu
public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
) : IRequest<Result>;