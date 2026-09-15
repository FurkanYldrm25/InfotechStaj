// Staj.Application/Features/Proposals/Queries/GetProposalById/GetProposalByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Proposals.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Proposals.Queries.GetProposalById;

// Teklif detay işleyicisi
public class GetProposalByIdQueryHandler
    : IRequestHandler<GetProposalByIdQuery, Result<ProposalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProposalByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ProposalDto>> Handle(
        GetProposalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<ProposalDto>.Fail("Yetkisiz erişim.");

        var proposal = await _context.Proposals
            .AsNoTracking()
            .Include(p => p.JobPost)
            .Include(p => p.FreelancerProfile).ThenInclude(f => f.User)
            .Include(p => p.ClientProfile).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (proposal is null)
            return Result<ProposalDto>.Fail("Teklif bulunamadı.");

        // Yetki: sadece iki taraf ya da admin (admin rolü ekstra kontrol gerektirmez;
        // burada yalnızca taraf kontrolü yapıyoruz — admin controller katmanında serbest bırakılır)
        var isFreelancer = proposal.FreelancerProfile.UserId == userId.Value;
        var isClient = proposal.ClientProfile.UserId == userId.Value;
        var isAdmin = _currentUser.Roles.Contains("Admin");

        if (!isFreelancer && !isClient && !isAdmin)
            return Result<ProposalDto>.Fail("Bu teklifi görüntüleme yetkiniz yok.");

        var dto = new ProposalDto(
            proposal.Id,
            proposal.Kind,
            proposal.Status,
            proposal.JobPostId,
            proposal.JobPost?.Title,
            proposal.FreelancerProfileId,
            proposal.FreelancerProfile.UserId,
            proposal.FreelancerProfile.User.FirstName,
            proposal.FreelancerProfile.User.LastName,
            proposal.FreelancerProfile.Title,
            proposal.ClientProfileId,
            proposal.ClientProfile.UserId,
            proposal.ClientProfile.CompanyName,
            proposal.ClientProfile.User.FirstName,
            proposal.ClientProfile.User.LastName,
            proposal.CoverMessage,
            proposal.ProposedRate,
            proposal.ProposedDurationDays,
            proposal.Currency,
            proposal.RespondedAt,
            proposal.ResponseNote,
            proposal.CreatedAt,
            proposal.UpdatedAt);

        return Result<ProposalDto>.Ok(dto);
    }
}