// Staj.Application/Features/Users/Commands/UpdateBasicInfo/UpdateBasicInfoCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Users.Commands.UpdateBasicInfo;

public class UpdateBasicInfoCommandValidator : AbstractValidator<UpdateBasicInfoCommand>
{
    public UpdateBasicInfoCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}