// Staj.Application/Features/Users/Commands/UpdateProfileImage/UpdateProfileImageCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Users.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("URL boş olamaz.")
            .MaximumLength(500);
    }
}