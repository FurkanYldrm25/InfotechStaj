// Staj.Application/Features/Messaging/Queries/GetUnreadCount/GetUnreadCountQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetUnreadCount;

// Toplam okunmamış mesaj sayısını getiren işleyici
public class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, Result<UnreadCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<UnreadCountDto>.Fail("Yetkisiz erişim.");

        var uid = userId.Value;

        var count = await _context.Messages
            .Where(m =>
                m.SenderId != uid &&
                !m.IsRead &&
                (m.Conversation.Participant1Id == uid || m.Conversation.Participant2Id == uid))
            .CountAsync(cancellationToken);

        return Result<UnreadCountDto>.Ok(new UnreadCountDto(count));
    }
}