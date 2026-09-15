// Staj.API/Controllers/MessagesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Messaging.Commands.MarkConversationAsRead;
using Staj.Application.Features.Messaging.Commands.SendMessage;
using Staj.Application.Features.Messaging.Commands.StartOrGetConversation;
using Staj.Application.Features.Messaging.Queries.GetConversationMessages;
using Staj.Application.Features.Messaging.Queries.GetMyConversations;
using Staj.Application.Features.Messaging.Queries.GetUnreadCount;

namespace Staj.API.Controllers;

// Mesajlaşma uçları
[ApiController]
[Route("api")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Conversation başlat veya var olanı getir
    [HttpPost("conversations")]
    public async Task<IActionResult> StartOrGet(
        [FromBody] StartOrGetConversationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kendi conversation'larını listele
    [HttpGet("conversations")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyConversationsQuery(page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Bir conversation'ın mesajları
    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetConversationMessagesQuery(id, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Conversation'ı okundu olarak işaretle
    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MarkConversationAsReadCommand(id),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Mesaj gönder
    [HttpPost("messages")]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Toplam okunmamış mesaj sayısı (badge için)
    [HttpGet("messages/unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), cancellationToken);
        return Ok(result);
    }
}