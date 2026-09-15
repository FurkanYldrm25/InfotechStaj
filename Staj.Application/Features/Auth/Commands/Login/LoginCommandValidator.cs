// Staj.Application/Features/Auth/Commands/Login/LoginCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Auth.Commands.Login;

// Giriş doğrulama kuralları
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("Geçerli bir email giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.");
    }
}