// Staj.Application/Features/Proposals/Commands/SubmitApplication/SubmitApplicationCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Commands.SubmitApplication;

// Başvuru gönderme işleyicisi
public class SubmitApplicationCommandHandler
    : IRequestHandler<SubmitApplicationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public SubmitApplicationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Result<Guid>> Handle(
        SubmitApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        var freelancerProfile = await _context.FreelancerProfiles
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.UserId == userId.Value, cancellationToken);

        if (freelancerProfile is null)
            return Result<Guid>.Fail("Önce Freelancer profilinizi oluşturun.");

        var jobPost = await _context.JobPosts
            .Include(j => j.ClientProfile)
            .FirstOrDefaultAsync(j => j.Id == request.JobPostId, cancellationToken);

        if (jobPost is null)
            return Result<Guid>.Fail("İlan bulunamadı.");

        if (jobPost.Status != JobPostStatus.Published)
            return Result<Guid>.Fail("Yalnızca yayındaki ilanlara başvurulabilir.");

        var alreadyApplied = await _context.Proposals
            .AnyAsync(p =>
                p.JobPostId == jobPost.Id &&
                p.FreelancerProfileId == freelancerProfile.Id &&
                p.Kind == ProposalKind.Application, cancellationToken);

        if (alreadyApplied)
            return Result<Guid>.Fail("Bu ilana daha önce başvurdunuz.");

        var proposal = new Proposal
        {
            JobPostId = jobPost.Id,
            FreelancerProfileId = freelancerProfile.Id,
            ClientProfileId = jobPost.ClientProfileId,
            Kind = ProposalKind.Application,
            Status = ProposalStatus.Pending,
            CoverMessage = request.CoverMessage.Trim(),
            ProposedRate = request.ProposedRate,
            ProposedDurationDays = request.ProposedDurationDays,
            Currency = request.Currency?.Trim()
        };

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);

        // Bildirim: Client'a yeni başvuru
        var fullName = $"{freelancerProfile.User.FirstName} {freelancerProfile.User.LastName}".Trim();
        await _notifications.CreateAndPushAsync(
            recipientUserId: jobPost.ClientProfile.UserId,
            type: NotificationType.NewApplication,
            title: "Yeni başvuru",
            body: $"{fullName}, '{jobPost.Title}' ilanınıza başvurdu.",
            referenceType: "Proposal",
            referenceId: proposal.Id,
            cancellationToken: cancellationToken);

        return Result<Guid>.Ok(proposal.Id, "Başvuru gönderildi.");
    }
}