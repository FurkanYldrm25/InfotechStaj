// Staj.API/Controllers/SkillsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Skills.Commands.CreateSkill;
using Staj.Application.Features.Skills.Commands.DeleteSkill;
using Staj.Application.Features.Skills.Commands.UpdateSkill;
using Staj.Application.Features.Skills.Queries.GetAllSkills;
using Staj.Application.Features.Skills.Queries.GetSkillById;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// Skill uçları
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Skill'leri listele (public)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllSkillsQuery(onlyActive), cancellationToken);
        return Ok(result);
    }

    // Skill detayı (public)
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSkillByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Yeni skill oluştur (Admin)
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSkillCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Skill güncelle (Admin)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSkillCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ve gövde id'leri uyuşmuyor.");

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Skill sil (Admin, soft delete)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSkillCommand(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}