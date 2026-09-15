// Staj.API/Controllers/JobsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.JobPosts.Commands.CancelJobPost;
using Staj.Application.Features.JobPosts.Commands.CloseJobPost;
using Staj.Application.Features.JobPosts.Commands.CreateJobPost;
using Staj.Application.Features.JobPosts.Commands.DeleteJobPost;
using Staj.Application.Features.JobPosts.Commands.PublishJobPost;
using Staj.Application.Features.JobPosts.Commands.UpdateJobPost;
using Staj.Application.Features.JobPosts.Queries.GetJobPostById;
using Staj.Application.Features.JobPosts.Queries.GetMyJobPosts;
using Staj.Application.Features.JobPosts.Queries.SearchJobPosts;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// İş ilanı uçları
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // İlan arama (public, yalnızca yayındakiler)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] List<Guid>? skillIds,
        [FromQuery] decimal? minBudget,
        [FromQuery] decimal? maxBudget,
        [FromQuery] WorkMode? workMode,
        [FromQuery] string? country,
        [FromQuery] string? city,
        [FromQuery] DateTime? publishedAfter,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchJobPostsQuery(
            keyword, categoryId, skillIds,
            minBudget, maxBudget, workMode,
            country, city, publishedAfter, sortBy,
            page, pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // Kendi ilanlarını getir (Client)
    [HttpGet("me")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> GetMine(
        [FromQuery] JobPostStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyJobPostsQuery(status, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // İlan detayı (public – yayındakiler için; sahibi taslak/kapalı da görebilir)
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJobPostByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Yeni ilan oluştur (Client)
    [HttpPost]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobPostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // İlan güncelle (yalnızca sahibi Client)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateJobPostCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ve gövde id'leri uyuşmuyor.");

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Yayınla
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PublishJobPostCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kapat
    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CloseJobPostCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // İptal et
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelJobPostCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Sil (soft delete)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteJobPostCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}