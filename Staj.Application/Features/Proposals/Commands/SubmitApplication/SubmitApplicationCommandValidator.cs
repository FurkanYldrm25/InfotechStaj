// Staj.Application/Features/Proposals/Commands/SubmitApplication/SubmitApplicationCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Proposals.Commands.SubmitApplication;

// Başvuru doğrulama kuralları
public class SubmitApplicationCommandValidator : AbstractValidator<SubmitApplicationCommand>
{
    public SubmitApplicationCommandValidator()
    {
        RuleFor(x => x.JobPostId).NotEmpty();

        RuleFor(x => x.CoverMessage)
            .NotEmpty()
            .MinimumLength(20)
            .MaximumLength(3000);

        RuleFor(x => x.ProposedRate)
            .GreaterThanOrEqualTo(0).When(x => x.ProposedRate.HasValue);

        RuleFor(x => x.ProposedDurationDays)
            .InclusiveBetween(1, 3650).When(x => x.ProposedDurationDays.HasValue);

        RuleFor(x => x.Currency).MaximumLength(10);
    }
}