// Staj.Application/Features/Reviews/Commands/DeleteReview/DeleteReviewCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Reviews.Commands.DeleteReview;

// Değerlendirme silme işleyicisi
public class DeleteReviewCommandHandler
    : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Fail("Yetkisiz erişim.");

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review is null)
            return Result.Fail("Değerlendirme bulunamadı.");

        var isAdmin = _currentUser.Roles.Contains(UserRoles.Admin);
        if (review.AuthorUserId != userId.Value && !isAdmin)
            return Result.Fail("Bu değerlendirmeyi silme yetkiniz yok.");

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Değerlendirme silindi.");
    }
}