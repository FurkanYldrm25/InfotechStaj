// Staj.API/Controllers/UsersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Users.Commands.UpdateBasicInfo;
using Staj.Application.Features.Users.Commands.UpdateProfileImage;
using Staj.Application.Features.Users.Queries.GetMe;

namespace Staj.API.Controllers;

// Kullanıcı uçları
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Aktif kullanıcı özeti
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMeQuery(), cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    // Ad-soyad güncelle
    [HttpPut("me/basic-info")]
    public async Task<IActionResult> UpdateBasicInfo(
        [FromBody] UpdateBasicInfoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Profil fotoğrafı URL güncelle
    [HttpPut("me/profile-image")]
    public async Task<IActionResult> UpdateProfileImage(
        [FromBody] UpdateProfileImageCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}