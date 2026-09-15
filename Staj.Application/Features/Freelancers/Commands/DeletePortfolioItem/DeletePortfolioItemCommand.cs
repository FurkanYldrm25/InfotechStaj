// Staj.Application/Features/Freelancers/Commands/DeletePortfolioItem/DeletePortfolioItemCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.DeletePortfolioItem;

// Portfolio kalemi sil (soft delete)
public record DeletePortfolioItemCommand(Guid Id) : IRequest<Result>;