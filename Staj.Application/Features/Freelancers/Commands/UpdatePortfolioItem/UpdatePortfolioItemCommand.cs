// Staj.Application/Features/Freelancers/Commands/UpdatePortfolioItem/UpdatePortfolioItemCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.UpdatePortfolioItem;

// Portfolio kalemi güncelle
public record UpdatePortfolioItemCommand(
    Guid Id,
    string Title,
    string? Description,
    string? ProjectUrl,
    string? ImageUrl,
    int DisplayOrder
) : IRequest<Result>;