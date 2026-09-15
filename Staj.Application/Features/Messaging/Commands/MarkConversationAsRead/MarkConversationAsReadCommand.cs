// Staj.Application/Features/Messaging/Commands/MarkConversationAsRead/MarkConversationAsReadCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Messaging.Commands.MarkConversationAsRead;

// Conversation'daki karşı taraf mesajlarını okundu işaretle
public record MarkConversationAsReadCommand(Guid ConversationId) : IRequest<Result>;