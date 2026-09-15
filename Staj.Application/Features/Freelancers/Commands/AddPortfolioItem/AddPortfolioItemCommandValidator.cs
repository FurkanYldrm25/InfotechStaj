// Staj.Application/Features/Freelancers/Commands/AddPortfolioItem/AddPortfolioItemCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Freelancers.Commands.AddPortfolioItem;

// Portfolio kalemi doğrulama
public class AddPortfolioItemCommandValidator : AbstractValidator<AddPortfolioItemCommand>
{
    public AddPortfolioItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectUrl).MaximumLength(500);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}