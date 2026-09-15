// Staj.Application/Features/Freelancers/Commands/AddPortfolioItem/AddPortfolioItemCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.AddPortfolioItem;

// Portfolio kalemi ekle
public record AddPortfolioItemCommand(
    string Title,
    string? Description,
    string? ProjectUrl,
    string? ImageUrl,
    int DisplayOrder
) : IRequest<Result<Guid>>;