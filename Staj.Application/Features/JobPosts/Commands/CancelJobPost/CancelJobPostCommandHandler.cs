// Staj.Application/Features/JobPosts/Commands/CancelJobPost/CancelJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.CancelJobPost;

// İş ilanı iptal işleyicisi
public class CancelJobPostCommandHandler
    : IRequestHandler<CancelJobPostCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        CancelJobPostCommand request,
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
            return Result.Fail("Bu ilanı iptal etme yetkiniz yok.");

        if (jobPost.Status is JobPostStatus.Closed or JobPostStatus.Cancelled)
            return Result.Fail("Bu ilan zaten kapalı veya iptal edilmiş.");

        jobPost.Status = JobPostStatus.Cancelled;
        jobPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("İlan iptal edildi.");
    }
}