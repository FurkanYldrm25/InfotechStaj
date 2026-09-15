// Staj.API/Controllers/CategoriesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staj.Application.Features.Categories.Commands.AssignSkill;
using Staj.Application.Features.Categories.Commands.CreateCategory;
using Staj.Application.Features.Categories.Commands.DeleteCategory;
using Staj.Application.Features.Categories.Commands.RemoveSkill;
using Staj.Application.Features.Categories.Commands.UpdateCategory;
using Staj.Application.Features.Categories.Queries.GetAllCategories;
using Staj.Application.Features.Categories.Queries.GetCategoryById;
using Staj.Application.Features.Skills.Queries.GetSkillsByCategory;
using Staj.Domain.Enums;

namespace Staj.API.Controllers;

// Kategori uçları
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Kategorileri listele (public)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery(onlyActive), cancellationToken);
        return Ok(result);
    }

    // Kategori detayı (public)
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kategoriye ait skill'leri listele (public)
    [HttpGet("{id:guid}/skills")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSkills(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSkillsByCategoryQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Yeni kategori oluştur (Admin)
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kategori güncelle (Admin)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ve gövde id'leri uyuşmuyor.");

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kategori sil (Admin, soft delete)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Kategoriye skill ata (Admin)
    [HttpPost("{categoryId:guid}/skills/{skillId:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> AssignSkill(
        Guid categoryId,
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignSkillToCategoryCommand(categoryId, skillId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Kategoriden skill kaldır (Admin)
    [HttpDelete("{categoryId:guid}/skills/{skillId:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> RemoveSkill(
        Guid categoryId,
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveSkillFromCategoryCommand(categoryId, skillId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}