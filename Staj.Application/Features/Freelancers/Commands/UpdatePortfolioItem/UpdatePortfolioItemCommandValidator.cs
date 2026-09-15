// Staj.Application/Features/Freelancers/Commands/UpdatePortfolioItem/UpdatePortfolioItemCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Freelancers.Commands.UpdatePortfolioItem;

public class UpdatePortfolioItemCommandValidator : AbstractValidator<UpdatePortfolioItemCommand>
{
    public UpdatePortfolioItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectUrl).MaximumLength(500);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}