// Staj.Application/Features/Freelancers/Commands/SetSkills/SetFreelancerSkillsCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Freelancers.Commands.SetSkills;

// Freelancer'ın skill listesini komple değiştirir
public record SetFreelancerSkillsCommand(List<FreelancerSkillInput> Skills)
    : IRequest<Result>;

public record FreelancerSkillInput(Guid SkillId, int? ProficiencyLevel);