// Staj.Application/Features/Reviews/Commands/DeleteReview/DeleteReviewCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Reviews.Commands.DeleteReview;

// Değerlendirme silme komutu (soft, kendi yorumu veya admin)
public record DeleteReviewCommand(Guid Id) : IRequest<Result>;