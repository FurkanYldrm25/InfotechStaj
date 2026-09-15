// Staj.Application/Features/Proposals/Commands/RejectProposal/RejectProposalCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Commands.RejectProposal;

// Teklif reddetme işleyicisi
public class RejectProposalCommandHandler
    : IRequestHandler<RejectProposalCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public RejectProposalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result> Handle(
        RejectProposalCommand request,
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
            return Result.Fail("Yalnızca bekleyen teklifler reddedilebilir.");

        var isAuthorized = proposal.Kind switch
        {
            ProposalKind.Application => proposal.ClientProfile.UserId == userId.Value,
            ProposalKind.DirectOffer => proposal.FreelancerProfile.UserId == userId.Value,
            _ => false
        };

        if (!isAuthorized)
            return Result.Fail("Bu teklifi reddetme yetkiniz yok.");

        proposal.Status = ProposalStatus.Rejected;
        proposal.RespondedAt = DateTime.UtcNow;
        proposal.ResponseNote = request.ResponseNote?.Trim();
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var recipientUserId = proposal.Kind == ProposalKind.Application
            ? proposal.FreelancerProfile.UserId
            : proposal.ClientProfile.UserId;

        var refText = proposal.JobPost is not null
            ? $"'{proposal.JobPost.Title}' ilanı için teklifiniz reddedildi."
            : "Doğrudan teklifiniz reddedildi.";

        await _notifications.CreateAndPushAsync(
            recipientUserId: recipientUserId,
            type: NotificationType.ProposalStatusChanged,
            title: "Teklif reddedildi",
            body: refText,
            referenceType: "Proposal",
            referenceId: proposal.Id,
            cancellationToken: cancellationToken);

        return Result.Ok("Teklif reddedildi.");
    }
}