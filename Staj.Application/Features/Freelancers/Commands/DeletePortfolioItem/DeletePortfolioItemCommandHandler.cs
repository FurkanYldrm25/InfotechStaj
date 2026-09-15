// Staj.Application/Features/Freelancers/Commands/DeletePortfolioItem/DeletePortfolioItemCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.DeletePortfolioItem;

public class DeletePortfolioItemCommandHandler
    : IRequestHandler<DeletePortfolioItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeletePortfolioItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeletePortfolioItemCommand request,
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

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Portfolio kalemi silindi.");
    }
}