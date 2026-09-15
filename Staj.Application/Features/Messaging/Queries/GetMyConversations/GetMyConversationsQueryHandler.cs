// Staj.Application/Features/Messaging/Queries/GetMyConversations/GetMyConversationsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetMyConversations;

// Conversation listeleme işleyicisi
public class GetMyConversationsQueryHandler
    : IRequestHandler<GetMyConversationsQuery, Result<PagedResult<ConversationListItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyConversationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ConversationListItemDto>>> Handle(
        GetMyConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<ConversationListItemDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var uid = userId.Value;

        var baseQuery = _context.Conversations
            .AsNoTracking()
            .Include(c => c.Participant1)
            .Include(c => c.Participant2)
            .Where(c => c.Participant1Id == uid || c.Participant2Id == uid);

        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConversationListItemDto(
                c.Id,
                c.Participant1Id == uid ? c.Participant2Id : c.Participant1Id,
                c.Participant1Id == uid ? c.Participant2.FirstName : c.Participant1.FirstName,
                c.Participant1Id == uid ? c.Participant2.LastName : c.Participant1.LastName,
                c.Participant1Id == uid ? c.Participant2.ProfileImageUrl : c.Participant1.ProfileImageUrl,
                c.LastMessagePreview,
                c.LastMessageSenderId,
                c.LastMessageAt,
                // Okunmamış sayı: karşı tarafın gönderdiği ve henüz okunmayanlar
                c.Messages.Count(m => m.SenderId != uid && !m.IsRead)))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ConversationListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<ConversationListItemDto>>.Ok(result);
    }
}