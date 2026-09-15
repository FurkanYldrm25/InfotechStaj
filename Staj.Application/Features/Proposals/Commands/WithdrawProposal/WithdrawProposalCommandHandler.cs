// Staj.Application/Features/Proposals/Commands/WithdrawProposal/WithdrawProposalCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Commands.WithdrawProposal;

// Teklif geri çekme işleyicisi
public class WithdrawProposalCommandHandler
    : IRequestHandler<WithdrawProposalCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public WithdrawProposalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result> Handle(
        WithdrawProposalCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var proposal = await _context.Proposals
            .Include(p => p.FreelancerProfile)
            .Include(p => p.ClientProfile)
            .Include(p => p.JobPost)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (proposal is null)
            return Result.Fail("Teklif bulunamadı.");

        if (proposal.Status != ProposalStatus.Pending)
            return Result.Fail("Yalnızca bekleyen teklifler geri çekilebilir.");

        // Yetki: Application'ı Freelancer, DirectOffer'ı Client geri çeker
        var isAuthorized = proposal.Kind switch
        {
            ProposalKind.Application => proposal.FreelancerProfile.UserId == userId.Value,
            ProposalKind.DirectOffer => proposal.ClientProfile.UserId == userId.Value,
            _ => false
        };

        if (!isAuthorized)
            return Result.Fail("Bu teklifi geri çekme yetkiniz yok.");

        proposal.Status = ProposalStatus.Withdrawn;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Bildirim: karşı tarafa "teklif geri çekildi"
        var recipientUserId = proposal.Kind == ProposalKind.Application
            ? proposal.ClientProfile.UserId
            : proposal.FreelancerProfile.UserId;

        var refText = proposal.JobPost is not null
            ? $"'{proposal.JobPost.Title}' ilanı için gönderilen teklif geri çekildi."
            : "Bir doğrudan teklif geri çekildi.";

        try
        {
            await _notifications.CreateAndPushAsync(
                recipientUserId: recipientUserId,
                type: NotificationType.ProposalStatusChanged,
                title: "Teklif geri çekildi",
                body: refText,
                referenceType: "Proposal",
                referenceId: proposal.Id,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Sessiz geç
        }

        return Result.Ok("Teklif geri çekildi.");
    }
}