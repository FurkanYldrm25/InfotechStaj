// Staj.API/Controllers/FreelancersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Freelancers.Commands.AddPortfolioItem;
using Staj.Application.Features.Freelancers.Commands.CreateOrUpdateProfile;
using Staj.Application.Features.Freelancers.Commands.DeletePortfolioItem;
using Staj.Application.Features.Freelancers.Commands.SetSkills;
using Staj.Application.Features.Freelancers.Commands.UpdatePortfolioItem;
using Staj.Application.Features.Freelancers.Queries.GetFreelancerById;
using Staj.Application.Features.Freelancers.Queries.GetMyFreelancerProfile;
using Staj.Application.Features.Freelancers.Queries.SearchFreelancers;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// Freelancer uçları
[ApiController]
[Route("api/[controller]")]
public class FreelancersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FreelancersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Freelancer arama (public)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] List<Guid>? skillIds,
        [FromQuery] decimal? minHourlyRate,
        [FromQuery] decimal? maxHourlyRate,
        [FromQuery] string? country,
        [FromQuery] string? city,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchFreelancersQuery(
            keyword, categoryId, skillIds,
            minHourlyRate, maxHourlyRate,
            country, city, isAvailable, page, pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // Freelancer detayı (public)
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFreelancerByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kendi freelancer profilini getir
    [HttpGet("me")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyFreelancerProfileQuery(), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kendi freelancer profilini oluştur/güncelle
    [HttpPut("me")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> UpsertMyProfile(
        [FromBody] CreateOrUpdateFreelancerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kendi skill listeni komple değiştir
    [HttpPut("me/skills")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> SetSkills(
        [FromBody] SetFreelancerSkillsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Portfolio kalemi ekle
    [HttpPost("me/portfolio")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> AddPortfolio(
        [FromBody] AddPortfolioItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Portfolio kalemi güncelle
    [HttpPut("me/portfolio/{id:guid}")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> UpdatePortfolio(
        Guid id,
        [FromBody] UpdatePortfolioItemCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ve gövde id'leri uyuşmuyor.");

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Portfolio kalemi sil
    [HttpDelete("me/portfolio/{id:guid}")]
    [Authorize(Roles = UserRoles.Freelancer + "," + UserRoles.Admin)]
    public async Task<IActionResult> DeletePortfolio(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeletePortfolioItemCommand(id), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}