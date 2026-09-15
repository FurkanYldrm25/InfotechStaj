// Staj.API/Controllers/ClientsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Clients.Commands.CreateOrUpdateProfile;
using Staj.Application.Features.Clients.Queries.GetClientById;
using Staj.Application.Features.Clients.Queries.GetMyClientProfile;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// Client uçları
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Client detayı (giriş yapmış herkes)
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kendi client profilini getir
    [HttpGet("me")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyClientProfileQuery(), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kendi client profilini oluştur/güncelle
    [HttpPut("me")]
    [Authorize(Roles = UserRoles.Client + "," + UserRoles.Admin)]
    public async Task<IActionResult> UpsertMyProfile(
        [FromBody] CreateOrUpdateClientProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}