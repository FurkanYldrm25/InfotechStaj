// Staj.Application/Features/Auth/Commands/Register/RegisterCommandValidator.cs
using FluentValidation;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Auth.Commands.Register;

// Kayıt komutu için doğrulama kuralları
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş olamaz.")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş olamaz.")
            .MaximumLength(50);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => r == UserRoles.Freelancer || r == UserRoles.Client)
            .WithMessage("Rol yalnızca Freelancer veya Client olabilir.");
    }
}