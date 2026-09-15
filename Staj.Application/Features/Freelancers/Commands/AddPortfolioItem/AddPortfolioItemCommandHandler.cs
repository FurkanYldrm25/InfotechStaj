// Staj.Application/Features/Freelancers/Commands/AddPortfolioItem/AddPortfolioItemCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Freelancers.Commands.AddPortfolioItem;

// Portfolio kalemi ekleyen işleyici
public class AddPortfolioItemCommandHandler
    : IRequestHandler<AddPortfolioItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddPortfolioItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        AddPortfolioItemCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result<Guid>.Fail("Yetkisiz erişim.");

        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId.Value, cancellationToken);

        if (profile is null)
            return Result<Guid>.Fail("Önce freelancer profilinizi oluşturun.");

        var item = new PortfolioItem
        {
            FreelancerProfileId = profile.Id,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ProjectUrl = request.ProjectUrl?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            DisplayOrder = request.DisplayOrder
        };

        _context.PortfolioItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(item.Id, "Portfolio kalemi eklendi.");
    }
}