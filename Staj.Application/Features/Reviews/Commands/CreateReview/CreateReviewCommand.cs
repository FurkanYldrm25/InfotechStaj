// Staj.Application/Features/Reviews/Commands/CreateReview/CreateReviewCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Reviews.Commands.CreateReview;

// Yeni değerlendirme oluşturma komutu
public record CreateReviewCommand(
    Guid ProposalId,
    int Rating,
    string Comment
) : IRequest<Result<Guid>>;