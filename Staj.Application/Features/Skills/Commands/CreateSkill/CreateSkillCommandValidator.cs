// Staj.Application/Features/Skills/Commands/CreateSkill/CreateSkillCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Skills.Commands.CreateSkill;

// Skill oluşturma doğrulama kuralları
public class CreateSkillCommandValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill adı boş olamaz.")
            .MinimumLength(1).WithMessage("Skill adı en az 1 karakter olmalıdır.")
            .MaximumLength(80).WithMessage("Skill adı en fazla 80 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}