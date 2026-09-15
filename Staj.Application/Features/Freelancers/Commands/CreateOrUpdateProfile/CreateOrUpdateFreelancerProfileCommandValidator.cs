// Staj.Application/Features/Freelancers/Commands/CreateOrUpdateProfile/CreateOrUpdateFreelancerProfileCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Freelancers.Commands.CreateOrUpdateProfile;

// Freelancer profil doğrulama kuralları
public class CreateOrUpdateFreelancerProfileCommandValidator
    : AbstractValidator<CreateOrUpdateFreelancerProfileCommand>
{
    public CreateOrUpdateFreelancerProfileCommandValidator()
    {
        RuleFor(x => x.Title).MaximumLength(150);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 60).When(x => x.ExperienceYears.HasValue);

        RuleFor(x => x.HourlyRateMin)
            .GreaterThanOrEqualTo(0).When(x => x.HourlyRateMin.HasValue);
        RuleFor(x => x.HourlyRateMax)
            .GreaterThanOrEqualTo(0).When(x => x.HourlyRateMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.HourlyRateMin.HasValue || !x.HourlyRateMax.HasValue
                       || x.HourlyRateMax >= x.HourlyRateMin)
            .WithMessage("Maksimum ücret minimum ücretten küçük olamaz.");

        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(80);
        RuleFor(x => x.CvUrl).MaximumLength(500);
        RuleFor(x => x.LinkedInUrl).MaximumLength(500);
        RuleFor(x => x.GitHubUrl).MaximumLength(500);
        RuleFor(x => x.WebsiteUrl).MaximumLength(500);
    }
}