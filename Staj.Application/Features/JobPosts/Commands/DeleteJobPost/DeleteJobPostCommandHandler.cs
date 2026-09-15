// Staj.Application/Features/JobPosts/Commands/DeleteJobPost/DeleteJobPostCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.JobPosts.Commands.DeleteJobPost;

// İş ilanı silme işleyicisi (soft delete)
public class DeleteJobPostCommandHandler
    : IRequestHandler<DeleteJobPostCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteJobPostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteJobPostCommand request,
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
            return Result.Fail("Bu ilanı silme yetkiniz yok.");

        jobPost.IsDeleted = true;
        jobPost.DeletedAt = DateTime.UtcNow;
        jobPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("İlan silindi.");
    }
}