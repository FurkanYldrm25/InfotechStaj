// Staj.Application/Features/Categories/Commands/AssignSkill/AssignSkillToCategoryCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.AssignSkill;

// Kategoriye skill atama komutu
public record AssignSkillToCategoryCommand(Guid CategoryId, Guid SkillId) : IRequest<Result>;