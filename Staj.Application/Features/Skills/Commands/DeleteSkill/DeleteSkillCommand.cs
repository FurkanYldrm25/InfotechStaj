// Staj.Application/Features/Skills/Commands/DeleteSkill/DeleteSkillCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Skills.Commands.DeleteSkill;

// Skill silme komutu (soft delete)
public record DeleteSkillCommand(Guid Id) : IRequest<Result>;