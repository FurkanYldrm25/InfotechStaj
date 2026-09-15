// Staj.Application/Features/Messaging/Commands/StartOrGetConversation/StartOrGetConversationCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Messaging.Commands.StartOrGetConversation;

// Conversation upsert işleyicisi
public class StartOrGetConversationCommandHandler
    : IRequestHandler<StartOrGetConversationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public StartOrGetConversationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        StartOrGetConversationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Guid>.Fail("Yetkisiz erişim.");

        if (userId.Value == request.OtherUserId)
            return Result<Guid>.Fail("Kendinizle conversation başlatamazsınız.");

        // Karşı taraf gerçekten var mı?
        var otherExists = await _context.Users
            .AnyAsync(u => u.Id == request.OtherUserId, cancellationToken);

        if (!otherExists)
            return Result<Guid>.Fail("Kullanıcı bulunamadı.");

        // Katılımcı çiftini normalize et: küçük id her zaman Participant1
        var (p1, p2) = userId.Value.CompareTo(request.OtherUserId) < 0
            ? (userId.Value, request.OtherUserId)
            : (request.OtherUserId, userId.Value);

        var existing = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                c.Participant1Id == p1 && c.Participant2Id == p2,
                cancellationToken);

        if (existing is not null)
            return Result<Guid>.Ok(existing.Id, "Mevcut conversation getirildi.");

        var conversation = new Conversation
        {
            Participant1Id = p1,
            Participant2Id = p2
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(conversation.Id, "Conversation oluşturuldu.");
    }
}