// Staj.Application/Features/JobPosts/Commands/CloseJobPost/CloseJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.CloseJobPost;

// İş ilanı kapatma işleyicisi
public class CloseJobPostCommandHandler
    : IRequestHandler<CloseJobPostCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CloseJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        CloseJobPostCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var jobPost = await _context.JobPosts
            .Include(j => j.ClientProfile)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobPost is null)
            return Result.Fail("İlan bulunamadı.");

        if (jobPost.ClientProfile.UserId != userId.Value)
            return Result.Fail("Bu ilanı kapatma yetkiniz yok.");

        if (jobPost.Status != JobPostStatus.Published)
            return Result.Fail("Yalnızca yayındaki ilanlar kapatılabilir.");

        jobPost.Status = JobPostStatus.Closed;
        jobPost.ClosedAt = DateTime.UtcNow;
        jobPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("İlan kapatıldı.");
    }
}