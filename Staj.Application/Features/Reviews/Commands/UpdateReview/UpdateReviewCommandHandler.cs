// Staj.Application/Features/Reviews/Commands/UpdateReview/UpdateReviewCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Reviews.Commands.UpdateReview;

// Değerlendirme güncelleme işleyicisi
public class UpdateReviewCommandHandler
    : IRequestHandler<UpdateReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Fail("Yetkisiz erişim.");

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review is null)
            return Result.Fail("Değerlendirme bulunamadı.");

        if (review.AuthorUserId != userId.Value)
            return Result.Fail("Bu değerlendirmeyi güncelleme yetkiniz yok.");

        review.Rating = request.Rating;
        review.Comment = request.Comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Değerlendirme güncellendi.");
    }
}