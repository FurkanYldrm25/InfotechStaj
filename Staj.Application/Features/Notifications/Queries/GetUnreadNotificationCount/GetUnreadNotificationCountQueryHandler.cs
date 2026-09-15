// Staj.Application/Features/Notifications/Queries/GetUnreadNotificationCount/GetUnreadNotificationCountQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Notifications.Dtos;

namespace Staj.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

// Kullanıcının okunmamış bildirim sayısı
public class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<UnreadCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationCountQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<UnreadCountDto>.Fail("Yetkisiz erişim.");

        var count = await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientUserId == userId.Value && !n.IsRead, cancellationToken);

        return Result<UnreadCountDto>.Ok(new UnreadCountDto(count));
    }
}