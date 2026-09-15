// Staj.Application/Features/Skills/Commands/UpdateSkill/UpdateSkillCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Skills.Commands.UpdateSkill;

// Skill güncelleme doğrulama kuralları
public class UpdateSkillCommandValidator : AbstractValidator<UpdateSkillCommand>
{
    public UpdateSkillCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill adı boş olamaz.")
            .MinimumLength(1)
            .MaximumLength(80).WithMessage("Skill adı en fazla 80 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}