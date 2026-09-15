// Staj.Application/Features/Notifications/Commands/MarkAllAsRead/MarkAllAsReadCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.MarkAllAsRead;

// Tüm bildirimleri okundu işaretle
public class MarkAllAsReadCommandHandler
    : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkAllAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Fail("Yetkisiz erişim.");

        var now = DateTime.UtcNow;

        var unread = await _context.Notifications
            .Where(n => n.RecipientUserId == userId.Value && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return Result.Ok("Okunmamış bildirim yok.");

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
            n.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok($"{unread.Count} bildirim okundu olarak işaretlendi.");
    }
}