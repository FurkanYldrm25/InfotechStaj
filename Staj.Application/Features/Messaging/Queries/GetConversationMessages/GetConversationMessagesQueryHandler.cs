// Staj.Application/Features/Messaging/Queries/GetConversationMessages/GetConversationMessagesQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetConversationMessages;

// Mesajları getiren işleyici (en yeni önce)
public class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, Result<PagedResult<MessageDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationMessagesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<MessageDto>>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<PagedResult<MessageDto>>.Fail("Yetkisiz erişim.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null)
            return Result<PagedResult<MessageDto>>.Fail("Conversation bulunamadı.");

        if (conversation.Participant1Id != userId.Value &&
            conversation.Participant2Id != userId.Value)
            return Result<PagedResult<MessageDto>>.Fail("Bu conversation'a erişim yetkiniz yok.");

        var query = _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversation.Id);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MessageDto(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.Sender.FirstName,
                m.Sender.LastName,
                m.Sender.ProfileImageUrl,
                m.Content,
                m.IsRead,
                m.ReadAt,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<MessageDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return Result<PagedResult<MessageDto>>.Ok(result);
    }
}