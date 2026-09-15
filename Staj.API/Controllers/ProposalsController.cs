// Staj.API/Controllers/ProposalsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Proposals.Commands.AcceptProposal;
using Staj.Application.Features.Proposals.Commands.RejectProposal;
using Staj.Application.Features.Proposals.Commands.SubmitApplication;
using Staj.Application.Features.Proposals.Commands.SubmitDirectOffer;
using Staj.Application.Features.Proposals.Commands.WithdrawProposal;
using Staj.Application.Features.Proposals.Queries.GetMyProposals;
using Staj.Application.Features.Proposals.Queries.GetProposalById;
using Staj.Application.Features.Proposals.Queries.GetProposalsForJobPost;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// Teklif ve başvuru uçları
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProposalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProposalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Freelancer'ın bir ilana başvurusu
    [HttpPost("applications")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> SubmitApplication(
        [FromBody] SubmitApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Client'ın freelancer'a doğrudan teklifi
    [HttpPost("direct-offers")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> SubmitDirectOffer(
        [FromBody] SubmitDirectOfferCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Teklif detayı (sadece taraflar veya admin)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProposalByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kendi tekliflerin (yön + tür + durum filtreli)
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] ProposalDirection direction,
        [FromQuery] ProposalKind? kind,
        [FromQuery] ProposalStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyProposalsQuery(direction, kind, status, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Bir ilanın başvuruları (yalnızca ilan sahibi Client)
    [HttpGet("for-job/{jobPostId:guid}")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> GetForJob(
        Guid jobPostId,
        [FromQuery] ProposalStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProposalsForJobPostQuery(jobPostId, status, page, pageSize),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kabul et
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(
        Guid id,
        [FromBody] AcceptProposalCommand? body,
        CancellationToken cancellationToken)
    {
        var command = new AcceptProposalCommand(id, body?.ResponseNote);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Reddet
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectProposalCommand? body,
        CancellationToken cancellationToken)
    {
        var command = new RejectProposalCommand(id, body?.ResponseNote);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Geri çek
    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new WithdrawProposalCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}