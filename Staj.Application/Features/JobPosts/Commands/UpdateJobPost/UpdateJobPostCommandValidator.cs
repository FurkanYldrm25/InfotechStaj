// Staj.Application/Features/JobPosts/Commands/UpdateJobPost/UpdateJobPostCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.JobPosts.Commands.UpdateJobPost;

// İş ilanı güncelleme doğrulama kuralları
public class UpdateJobPostCommandValidator : AbstractValidator<UpdateJobPostCommand>
{
    public UpdateJobPostCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(20)
            .MaximumLength(5000);

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0).When(x => x.BudgetMin.HasValue);
        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0).When(x => x.BudgetMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.BudgetMin.HasValue || !x.BudgetMax.HasValue
                       || x.BudgetMax >= x.BudgetMin)
            .WithMessage("Maksimum bütçe minimum bütçeden küçük olamaz.");

        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(80);
        RuleFor(x => x.DurationDays)
            .InclusiveBetween(1, 3650).When(x => x.DurationDays.HasValue);

        RuleFor(x => x.WorkMode).IsInEnum();

        RuleFor(x => x.SkillIds)
            .NotNull()
            .Must(list => list.Count <= 20)
            .WithMessage("En fazla 20 skill seçilebilir.");
    }
}