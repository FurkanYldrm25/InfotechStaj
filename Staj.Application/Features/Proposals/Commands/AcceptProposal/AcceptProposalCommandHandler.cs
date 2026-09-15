// Staj.Application/Features/Proposals/Commands/AcceptProposal/AcceptProposalCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Commands.AcceptProposal;

// Teklif kabul işleyicisi
public class AcceptProposalCommandHandler
    : IRequestHandler<AcceptProposalCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public AcceptProposalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result> Handle(
        AcceptProposalCommand request,
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
            return Result.Fail("Yalnızca bekleyen teklifler kabul edilebilir.");

        var isAuthorized = proposal.Kind switch
        {
            ProposalKind.Application => proposal.ClientProfile.UserId == userId.Value,
            ProposalKind.DirectOffer => proposal.FreelancerProfile.UserId == userId.Value,
            _ => false
        };

        if (!isAuthorized)
            return Result.Fail("Bu teklifi kabul etme yetkiniz yok.");

        proposal.Status = ProposalStatus.Accepted;
        proposal.RespondedAt = DateTime.UtcNow;
        proposal.ResponseNote = request.ResponseNote?.Trim();
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Bildirim: teklifi gönderen karşı tarafa
        var recipientUserId = proposal.Kind == ProposalKind.Application
            ? proposal.FreelancerProfile.UserId
            : proposal.ClientProfile.UserId;

        var refText = proposal.JobPost is not null
            ? $"'{proposal.JobPost.Title}' ilanı için teklifiniz kabul edildi."
            : "Doğrudan teklifiniz kabul edildi.";

        await _notifications.CreateAndPushAsync(
            recipientUserId: recipientUserId,
            type: NotificationType.ProposalStatusChanged,
            title: "Teklif kabul edildi",
            body: refText,
            referenceType: "Proposal",
            referenceId: proposal.Id,
            cancellationToken: cancellationToken);

        return Result.Ok("Teklif kabul edildi.");
    }
}