// Staj.Application/Features/Messaging/Commands/SendMessage/SendMessageCommandValidator.cs
using FluentValidation;

namespace Staj.Application.Features.Messaging.Commands.SendMessage;

// Mesaj doğrulama kuralları
public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}