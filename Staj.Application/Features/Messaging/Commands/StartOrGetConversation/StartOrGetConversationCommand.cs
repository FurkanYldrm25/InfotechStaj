// Staj.Application/Features/Messaging/Commands/StartOrGetConversation/StartOrGetConversationCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Messaging.Commands.StartOrGetConversation;

// Belirtilen kullanıcı ile conversation'ı bulur, yoksa oluşturur (idempotent)
public record StartOrGetConversationCommand(Guid OtherUserId) : IRequest<Result<Guid>>;