// Staj.Application/Features/Notifications/Commands/DeleteNotification/DeleteNotificationCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.DeleteNotification;

// Bildirim silme işleyicisi
public class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteNotificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Fail("Yetkisiz erişim.");

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (notification is null)
            return Result.Fail("Bildirim bulunamadı.");

        if (notification.RecipientUserId != userId.Value)
            return Result.Fail("Bu bildirimi silme yetkiniz yok.");

        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Bildirim silindi.");
    }
}