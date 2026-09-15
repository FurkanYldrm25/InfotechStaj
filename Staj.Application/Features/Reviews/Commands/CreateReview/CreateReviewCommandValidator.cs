// Staj.Application/Features/Reviews/Commands/CreateReview/CreateReviewCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Reviews.Commands.CreateReview;

// Değerlendirme oluşturma doğrulama kuralları
public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProposalId).NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Puan 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000);
    }
}