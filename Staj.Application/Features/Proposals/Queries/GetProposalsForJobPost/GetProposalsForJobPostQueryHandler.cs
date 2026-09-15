// Staj.Application/Features/Proposals/Queries/GetProposalsForJobPost/GetProposalsForJobPostQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Queries.GetProposalsForJobPost;

// Bir ilanın aldığı başvuruları listeleyen işleyici
public class GetProposalsForJobPostQueryHandler
    : IRequestHandler<GetProposalsForJobPostQuery, Result<PagedResult<ProposalListItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProposalsForJobPostQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ProposalListItemDto>>> Handle(
        GetProposalsForJobPostQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<ProposalListItemDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var jobPost = await _context.JobPosts
            .Include(j => j.ClientProfile)
            .FirstOrDefaultAsync(j => j.Id == request.JobPostId, cancellationToken);

        if (jobPost is null)
            return Result<PagedResult<ProposalListItemDto>>.Fail("İlan bulunamadı.");

        // Sadece ilan sahibi client görebilir
        if (jobPost.ClientProfile.UserId != userId.Value)
            return Result<PagedResult<ProposalListItemDto>>.Fail("Bu ilanın başvurularını görüntüleme yetkiniz yok.");

        var query = _context.Proposals
            .AsNoTracking()
            .Include(p => p.JobPost)
            .Include(p => p.FreelancerProfile).ThenInclude(f => f.User)
            .Include(p => p.ClientProfile).ThenInclude(c => c.User)
            .Where(p =>
                p.Kind == ProposalKind.Application &&
                p.JobPostId == jobPost.Id);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
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