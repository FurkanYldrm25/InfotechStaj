// Staj.Application/Features/Clients/Commands/CreateOrUpdateProfile/CreateOrUpdateClientProfileCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Clients.Commands.CreateOrUpdateProfile;

public class CreateOrUpdateClientProfileCommandValidator
    : AbstractValidator<CreateOrUpdateClientProfileCommand>
{
    public CreateOrUpdateClientProfileCommandValidator()
    {
        RuleFor(x => x.CompanyName).MaximumLength(150);
        RuleFor(x => x.Industry).MaximumLength(100);
        RuleFor(x => x.About).MaximumLength(2000);
        RuleFor(x => x.WebsiteUrl).MaximumLength(500);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(80);
        RuleFor(x => x.ContactPhone).MaximumLength(30);
    }
}