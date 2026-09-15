// Staj.Application/Features/Reviews/Commands/CreateReview/CreateReviewCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Reviews.Commands.CreateReview;

// Değerlendirme oluşturma işleyicisi
public class CreateReviewCommandHandler
    : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public CreateReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result<Guid>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        var proposal = await _context.Proposals
            .Include(p => p.FreelancerProfile)
            .Include(p => p.ClientProfile)
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

        if (proposal is null)
            return Result<Guid>.Fail("Teklif bulunamadı.");

        if (proposal.Status != ProposalStatus.Accepted)
            return Result<Guid>.Fail("Yalnızca kabul edilmiş teklifler üzerinden değerlendirme yapılabilir.");

        var freelancerUserId = proposal.FreelancerProfile.UserId;
        var clientUserId = proposal.ClientProfile.UserId;

        Guid targetUserId;
        if (userId.Value == freelancerUserId)
            targetUserId = clientUserId;
        else if (userId.Value == clientUserId)
            targetUserId = freelancerUserId;
        else
            return Result<Guid>.Fail("Bu teklif için değerlendirme yapma yetkiniz yok.");

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r =>
                r.ProposalId == proposal.Id &&
                r.AuthorUserId == userId.Value, cancellationToken);

        if (alreadyReviewed)
            return Result<Guid>.Fail("Bu teklif için zaten bir değerlendirme yazdınız.");

        var review = new Review
        {
            ProposalId = proposal.Id,
            AuthorUserId = userId.Value,
            TargetUserId = targetUserId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            IsVisible = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        // Bildirim: değerlendirilen kullanıcıya
        await _notifications.CreateAndPushAsync(
            recipientUserId: targetUserId,
            type: NotificationType.NewReview,
            title: "Yeni değerlendirme",
            body: $"Size {review.Rating} yıldızlı yeni bir değerlendirme yazıldı.",
            referenceType: "Review",
            referenceId: review.Id,
            cancellationToken: cancellationToken);

        return Result<Guid>.Ok(review.Id, "Değerlendirme oluşturuldu.");
    }
}