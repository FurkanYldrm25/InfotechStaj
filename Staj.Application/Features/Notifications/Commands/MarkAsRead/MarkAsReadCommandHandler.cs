// Staj.Application/Features/Notifications/Commands/MarkAsRead/MarkAsReadCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.MarkAsRead;

// Bildirim okundu işleyicisi
public class MarkAsReadCommandHandler
    : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        MarkAsReadCommand request,
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
            return Result.Fail("Bu bildirime erişim yetkiniz yok.");

        if (notification.IsRead)
            return Result.Ok("Bildirim zaten okunmuş.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok("Bildirim okundu olarak işaretlendi.");
    }
}