// Staj.Application/Features/Proposals/Commands/SubmitDirectOffer/SubmitDirectOfferCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Proposals.Commands.SubmitDirectOffer;

// Doğrudan teklif doğrulama kuralları
public class SubmitDirectOfferCommandValidator : AbstractValidator<SubmitDirectOfferCommand>
{
    public SubmitDirectOfferCommandValidator()
    {
        RuleFor(x => x.FreelancerProfileId).NotEmpty();

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