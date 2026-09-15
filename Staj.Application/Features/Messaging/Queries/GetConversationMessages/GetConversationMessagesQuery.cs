// Staj.Application/Features/Messaging/Queries/GetConversationMessages/GetConversationMessagesQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetConversationMessages;

// Bir conversation'ın mesajlarını sayfalı olarak getirir
public record GetConversationMessagesQuery(
    Guid ConversationId,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<MessageDto>>>;