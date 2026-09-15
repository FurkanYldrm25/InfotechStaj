// Staj.Application/Features/Proposals/Queries/GetMyProposals/GetMyProposalsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Queries.GetMyProposals;

// Kullanıcının tekliflerini yön/tür/duruma göre getirir
public class GetMyProposalsQueryHandler
    : IRequestHandler<GetMyProposalsQuery, Result<PagedResult<ProposalListItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyProposalsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ProposalListItemDto>>> Handle(
        GetMyProposalsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<ProposalListItemDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        // Kullanıcının profil id'lerini bul (Freelancer ve/veya Client olabilir)
        var freelancerProfileId = await _context.FreelancerProfiles
            .Where(f => f.UserId == userId.Value)
            .Select(f => (Guid?)f.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var clientProfileId = await _context.ClientProfiles
            .Where(c => c.UserId == userId.Value)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        // Yön + Kind kombinasyonuna göre kimin tarafında olduğunu belirle
        // Sent + Application → Freelancer
        // Sent + DirectOffer → Client
        // Received + Application → Client (ilan sahibi)
        // Received + DirectOffer → Freelancer
        // Kind belirtilmezse: her iki tarafın gönderdiği/aldığı da dahil edilir
        IQueryable<Proposal> query = _context.Proposals
            .AsNoTracking()
            .Include(p => p.JobPost)
            .Include(p => p.FreelancerProfile).ThenInclude(f => f.User)
            .Include(p => p.ClientProfile).ThenInclude(c => c.User);

        query = (request.Direction, request.Kind) switch
        {
            (ProposalDirection.Sent, ProposalKind.Application) =>
                freelancerProfileId is null
                    ? query.Where(p => false)
                    : query.Where(p =>
                        p.Kind == ProposalKind.Application &&
                        p.FreelancerProfileId == freelancerProfileId),

            (ProposalDirection.Sent, ProposalKind.DirectOffer) =>
                clientProfileId is null
                    ? query.Where(p => false)
                    : query.Where(p =>
                        p.Kind == ProposalKind.DirectOffer &&
                        p.ClientProfileId == clientProfileId),

            (ProposalDirection.Received, ProposalKind.Application) =>
                clientProfileId is null
                    ? query.Where(p => false)
                    : query.Where(p =>
                        p.Kind == ProposalKind.Application &&
                        p.ClientProfileId == clientProfileId),

            (ProposalDirection.Received, ProposalKind.DirectOffer) =>
                freelancerProfileId is null
                    ? query.Where(p => false)
                    : query.Where(p =>
                        p.Kind == ProposalKind.DirectOffer &&
                        p.FreelancerProfileId == freelancerProfileId),

            (ProposalDirection.Sent, null) =>
                query.Where(p =>
                    (p.Kind == ProposalKind.Application && p.FreelancerProfileId == freelancerProfileId) ||
                    (p.Kind == ProposalKind.DirectOffer && p.ClientProfileId == clientProfileId)),

            (ProposalDirection.Received, null) =>
                query.Where(p =>
                    (p.Kind == ProposalKind.Application && p.ClientProfileId == clientProfileId) ||
                    (p.Kind == ProposalKind.DirectOffer && p.FreelancerProfileId == freelancerProfileId)),

            _ => query.Where(p => false)
        };

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProposalListItemDto(
                p.Id,
                p.Kind,
                p.Status,
                p.JobPostId,
                p.JobPost != null ? p.JobPost.Title : null,
                p.FreelancerProfileId,
                p.FreelancerProfile.User.FirstName,
                p.FreelancerProfile.User.LastName,
                p.FreelancerProfile.Title,
                p.ClientProfileId,
                p.ClientProfile.CompanyName,
                p.ClientProfile.User.FirstName,
                p.ClientProfile.User.LastName,
                p.ProposedRate,
                p.ProposedDurationDays,
                p.Currency,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProposalListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<ProposalListItemDto>>.Ok(result);
    }
}