// Staj.Application/Features/Categories/Commands/CreateCategory/CreateCategoryCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.CreateCategory;

// Kategori oluşturma komutu
public record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<Result<Guid>>;