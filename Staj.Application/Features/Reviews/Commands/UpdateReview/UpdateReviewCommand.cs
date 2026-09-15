// Staj.Application/Features/Reviews/Commands/UpdateReview/UpdateReviewCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Reviews.Commands.UpdateReview;

// Değerlendirme güncelleme komutu (sadece kendi yorumu)
public record UpdateReviewCommand(
    Guid Id,
    int Rating,
    string Comment
) : IRequest<Result>;