// Staj.API/Controllers/AuthController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Auth.Commands.Login;
using Staj.Application.Features.Auth.Commands.RefreshToken;
using Staj.Application.Features.Auth.Commands.Register;

namespace Staj.API.Controllers;

// Kimlik doğrulama uçları
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Yeni kullanıcı kaydı
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kullanıcı girişi
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    // Refresh token ile yenileme
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}