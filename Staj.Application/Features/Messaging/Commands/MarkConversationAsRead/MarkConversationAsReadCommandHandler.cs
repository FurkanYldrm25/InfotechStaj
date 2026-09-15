// Staj.Application/Features/Messaging/Commands/MarkConversationAsRead/MarkConversationAsReadCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Messaging.Commands.MarkConversationAsRead;

// Conversation'ı okundu olarak işaretle
public class MarkConversationAsReadCommandHandler
    : IRequestHandler<MarkConversationAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IChatNotifier _chatNotifier;

    public MarkConversationAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IChatNotifier chatNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _chatNotifier = chatNotifier;
    }

    public async Task<Result> Handle(
        MarkConversationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Result.Fail("Yetkisiz erişim.");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Fail("Conversation bulunamadı.");

        if (conversation.Participant1Id != userId.Value &&
            conversation.Participant2Id != userId.Value)
            return Result.Fail("Bu conversation'a erişim yetkiniz yok.");

        var otherUserId = conversation.Participant1Id == userId.Value
            ? conversation.Participant2Id
            : conversation.Participant1Id;

        var now = DateTime.UtcNow;

        // Karşı tarafın gönderdiği okunmamış mesajları güncelle
        var unread = await _context.Messages
            .Where(m =>
                m.ConversationId == conversation.Id &&
                m.SenderId != userId.Value &&
                !m.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return Result.Ok("Okunacak mesaj yok.");

        foreach (var m in unread)
        {
            m.IsRead = true;
            m.ReadAt = now;
            m.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Karşı tarafa "okundu" bildirimi gönder
        try
        {
            await _chatNotifier.NotifyReadAsync(otherUserId, conversation.Id, now, cancellationToken);
        }
        catch
        {
            // Sessiz geç
        }

        return Result.Ok($"{unread.Count} mesaj okundu olarak işaretlendi.");
    }
}