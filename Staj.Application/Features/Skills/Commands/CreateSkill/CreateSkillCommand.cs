// Staj.Application/Features/Skills/Commands/CreateSkill/CreateSkillCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Skills.Commands.CreateSkill;

// Skill oluşturma komutu
public record CreateSkillCommand(
    string Name,
    string? Description,
    List<Guid>? CategoryIds
) : IRequest<Result<Guid>>;