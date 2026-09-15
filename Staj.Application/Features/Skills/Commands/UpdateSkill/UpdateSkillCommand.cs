// Staj.Application/Features/Skills/Commands/UpdateSkill/UpdateSkillCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Skills.Commands.UpdateSkill;

// Skill güncelleme komutu
public record UpdateSkillCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
) : IRequest<Result>;