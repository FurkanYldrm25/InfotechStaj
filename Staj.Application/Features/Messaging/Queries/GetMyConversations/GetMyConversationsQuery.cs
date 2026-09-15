// Staj.Application/Features/Messaging/Queries/GetMyConversations/GetMyConversationsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetMyConversations;

// Kullanıcının conversation listesini getirir
public record GetMyConversationsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ConversationListItemDto>>>;