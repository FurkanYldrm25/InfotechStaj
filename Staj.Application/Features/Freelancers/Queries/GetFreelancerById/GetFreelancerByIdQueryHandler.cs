// Staj.Application/Features/Freelancers/Queries/GetFreelancerById/GetFreelancerByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;
using Staj.Application.Features.Freelancers.Queries.GetMyFreelancerProfile;

namespace Staj.Application.Features.Freelancers.Queries.GetFreelancerById;

public class GetFreelancerByIdQueryHandler
    : IRequestHandler<GetFreelancerByIdQuery, Result<FreelancerProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFreelancerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FreelancerProfileDto>> Handle(
        GetFreelancerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.FreelancerProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.FreelancerSkills).ThenInclude(fs => fs.Skill)
            .Include(p => p.PortfolioItems)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile is null)
            return Result<FreelancerProfileDto>.Fail("Freelancer bulunamadı.");

        var (average, count, recent) = await GetMyFreelancerProfileQueryHandler
            .LoadRatingAsync(_context, profile.UserId, cancellationToken);

        return Result<FreelancerProfileDto>.Ok(
            GetMyFreelancerProfileQueryHandler.MapToDto(profile, average, count, recent));
    }
}