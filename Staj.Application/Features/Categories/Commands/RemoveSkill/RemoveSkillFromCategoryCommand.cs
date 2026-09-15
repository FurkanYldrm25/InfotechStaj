// Staj.Application/Features/Categories/Commands/RemoveSkill/RemoveSkillFromCategoryCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Categories.Commands.RemoveSkill;

// Kategoriden skill kaldırma komutu
public record RemoveSkillFromCategoryCommand(Guid CategoryId, Guid SkillId) : IRequest<Result>;