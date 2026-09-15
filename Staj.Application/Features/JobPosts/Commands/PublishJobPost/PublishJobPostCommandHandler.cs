// Staj.Application/Features/JobPosts/Commands/PublishJobPost/PublishJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.PublishJobPost;

// İş ilanı yayınlama işleyicisi
public class PublishJobPostCommandHandler
    : IRequestHandler<PublishJobPostCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublishJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        PublishJobPostCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var jobPost = await _context.JobPosts
            .Include(j => j.ClientProfile)
            .Include(j => j.JobPostSkills)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobPost is null)
            return Result.Fail("İlan bulunamadı.");

        if (jobPost.ClientProfile.UserId != userId.Value)
            return Result.Fail("Bu ilanı yayınlama yetkiniz yok.");

        if (jobPost.Status != JobPostStatus.Draft)
            return Result.Fail("Yalnızca taslak durumdaki ilanlar yayınlanabilir.");

        if (jobPost.JobPostSkills.Count == 0)
            return Result.Fail("En az bir skill eklemeden ilan yayınlanamaz.");

        jobPost.Status = JobPostStatus.Published;
        jobPost.PublishedAt = DateTime.UtcNow;
        jobPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("İlan yayına alındı.");
    }
}