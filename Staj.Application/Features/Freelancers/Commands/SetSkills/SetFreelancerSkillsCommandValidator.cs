// Staj.Application/Features/Freelancers/Commands/SetSkills/SetFreelancerSkillsCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Freelancers.Commands.SetSkills;

// Skill set doğrulama
public class SetFreelancerSkillsCommandValidator : AbstractValidator<SetFreelancerSkillsCommand>
{
    public SetFreelancerSkillsCommandValidator()
    {
        RuleFor(x => x.Skills).NotNull();
        RuleForEach(x => x.Skills).ChildRules(child =>
        {
            child.RuleFor(s => s.SkillId).NotEmpty();
            child.RuleFor(s => s.ProficiencyLevel)
                .InclusiveBetween(1, 5)
                .When(s => s.ProficiencyLevel.HasValue);
        });
    }
}