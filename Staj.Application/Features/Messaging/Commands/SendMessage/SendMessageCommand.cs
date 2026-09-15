// Staj.Application/Features/Messaging/Commands/SendMessage/SendMessageCommand.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Commands.SendMessage;

// Mesaj gönderme komutu
public record SendMessageCommand(
    Guid ConversationId,
    string Content
) : IRequest<Result<MessageDto>>;