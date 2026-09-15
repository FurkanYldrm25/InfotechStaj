// Staj.API/Controllers/ReviewsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Reviews.Commands.CreateReview;
using Staj.Application.Features.Reviews.Commands.DeleteReview;
using Staj.Application.Features.Reviews.Commands.UpdateReview;
using Staj.Application.Features.Reviews.Queries.GetMyGivenReviews;
using Staj.Application.Features.Reviews.Queries.GetMyReceivedReviews;
using Staj.Application.Features.Reviews.Queries.GetReviewsForUser;
using Staj.Application.Features.Reviews.Queries.GetUserRatingSummary;

namespace Staj.API.Controllers;

// Değerlendirme uçları
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Bir kullanıcının aldığı değerlendirmeler (public)
    [HttpGet("for-user/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetReviewsForUserQuery(userId, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kullanıcı puan özeti (public)
    [HttpGet("for-user/{userId:guid}/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSummary(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserRatingSummaryQuery(userId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kendi yazdığın değerlendirmeler
    [HttpGet("me/given")]
    [Authorize]
    public async Task<IActionResult> GetMyGiven(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyGivenReviewsQuery(page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Sana yapılan değerlendirmeler
    [HttpGet("me/received")]
    [Authorize]
    public async Task<IActionResult> GetMyReceived(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyReceivedReviewsQuery(page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Değerlendirme oluştur
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Değerlendirme güncelle (sadece kendi yorumu)
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateReviewCommand body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(id, body.Rating, body.Comment);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Değerlendirme sil (kendi veya admin)
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteReviewCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}