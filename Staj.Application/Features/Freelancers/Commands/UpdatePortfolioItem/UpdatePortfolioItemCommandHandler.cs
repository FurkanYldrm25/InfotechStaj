// Staj.Application/Features/Freelancers/Commands/UpdatePortfolioItem/UpdatePortfolioItemCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.UpdatePortfolioItem;

public class UpdatePortfolioItemCommandHandler
    : IRequestHandler<UpdatePortfolioItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePortfolioItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        UpdatePortfolioItemCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var item = await _context.PortfolioItems
            .Include(p => p.FreelancerProfile)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (item is null)
            return Result.Fail("Portfolio kalemi bulunamadı.");

        if (item.FreelancerProfile.UserId != userId.Value)
            return Result.Fail("Bu kalem üzerinde yetkiniz yok.");

        item.Title = request.Title.Trim();
        item.Description = request.Description?.Trim();
        item.ProjectUrl = request.ProjectUrl?.Trim();
        item.ImageUrl = request.ImageUrl?.Trim();
        item.DisplayOrder = request.DisplayOrder;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Portfolio kalemi güncellendi.");
    }
}