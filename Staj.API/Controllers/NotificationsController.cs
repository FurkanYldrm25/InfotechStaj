// Staj.API/Controllers/NotificationsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Notifications.Commands.DeleteNotification;
using Staj.Application.Features.Notifications.Commands.MarkAllAsRead;
using Staj.Application.Features.Notifications.Commands.MarkAsRead;
using Staj.Application.Features.Notifications.Queries.GetMyNotifications;
using Staj.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

namespace Staj.API.Controllers;

// Bildirim uçları
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Kendi bildirimlerin (sayfalı, opsiyonel sadece okunmamış)
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] bool? onlyUnread,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyNotificationsQuery(onlyUnread, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Okunmamış bildirim sayacı
    [HttpGet("me/unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUnreadNotificationCountQuery(),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Tek bildirimi okundu işaretle
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Tüm bildirimleri okundu işaretle
    [HttpPost("me/read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Bildirimi sil (soft)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}