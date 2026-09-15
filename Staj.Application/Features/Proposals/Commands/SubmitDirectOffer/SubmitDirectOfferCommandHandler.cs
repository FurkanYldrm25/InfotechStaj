// Staj.Application/Features/Proposals/Commands/SubmitDirectOffer/SubmitDirectOfferCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Commands.SubmitDirectOffer;

// Doğrudan teklif gönderme işleyicisi
public class SubmitDirectOfferCommandHandler
    : IRequestHandler<SubmitDirectOfferCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public SubmitDirectOfferCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result<Guid>> Handle(
        SubmitDirectOfferCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        // Teklifi gönderen kullanıcının Client profili olmalı
        var clientProfile = await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);

        if (clientProfile is null)
            return Result<Guid>.Fail("Önce Client profilinizi oluşturun.");

        // Hedef freelancer var mı ve müsait mi?
        var freelancerProfile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(f => f.Id == request.FreelancerProfileId, cancellationToken);

        if (freelancerProfile is null)
            return Result<Guid>.Fail("Freelancer bulunamadı.");

        if (!freelancerProfile.IsAvailable)
            return Result<Guid>.Fail("Bu freelancer şu anda teklif kabul etmiyor.");

        // Aynı client → aynı freelancer, bekleyen açık teklif olmamalı
        var hasPending = await _context.Proposals
            .AnyAsync(p =>
                p.Kind == ProposalKind.DirectOffer &&
                p.ClientProfileId == clientProfile.Id &&
                p.FreelancerProfileId == freelancerProfile.Id &&
                p.Status == ProposalStatus.Pending, cancellationToken);

        if (hasPending)
            return Result<Guid>.Fail("Bu freelancer'a zaten bekleyen bir teklifiniz var.");

        var proposal = new Proposal
        {
            JobPostId = null,
            FreelancerProfileId = freelancerProfile.Id,
            ClientProfileId = clientProfile.Id,
            Kind = ProposalKind.DirectOffer,
            Status = ProposalStatus.Pending,
            CoverMessage = request.CoverMessage.Trim(),
            ProposedRate = request.ProposedRate,
            ProposedDurationDays = request.ProposedDurationDays,
            Currency = request.Currency?.Trim()
        };

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);

        // Bildirim: Freelancer'a doğrudan teklif
        var senderName = !string.IsNullOrWhiteSpace(clientProfile.CompanyName)
            ? clientProfile.CompanyName!
            : $"{clientProfile.User.FirstName} {clientProfile.User.LastName}".Trim();

        try
        {
            await _notifications.CreateAndPushAsync(
                recipientUserId: freelancerProfile.UserId,
                type: NotificationType.NewDirectOffer,
                title: "Yeni doğrudan teklif",
                body: $"{senderName} size doğrudan bir teklif gönderdi.",
                referenceType: "Proposal",
                referenceId: proposal.Id,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Bildirim patlarsa teklif kayıt akışı etkilenmesin
        }

        return Result<Guid>.Ok(proposal.Id, "Doğrudan teklif gönderildi.");
    }
}