// Staj.Application/Features/Notifications/Queries/GetMyNotifications/GetMyNotificationsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Notifications.Dtos;

namespace Staj.Application.Features.Notifications.Queries.GetMyNotifications;

// Kullanıcının kendi bildirimlerini getirir
public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<NotificationDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId.Value);

        if (request.OnlyUnread == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Body,
                n.ReferenceType, n.ReferenceId,
                n.IsRead, n.ReadAt, n.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<NotificationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<NotificationDto>>.Ok(result);
    }
}