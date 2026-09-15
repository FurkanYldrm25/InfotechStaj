// Staj.Application/Features/Categories/Commands/DeleteCategory/DeleteCategoryCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.DeleteCategory;

// Kategori silme komutu (soft delete)
public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;