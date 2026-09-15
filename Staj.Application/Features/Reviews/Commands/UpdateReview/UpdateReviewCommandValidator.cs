// Staj.Application/Features/Reviews/Commands/UpdateReview/UpdateReviewCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Reviews.Commands.UpdateReview;

// Güncelleme doğrulama kuralları
public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Puan 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000);
    }
}