// Staj.Application/Features/Messaging/Commands/SendMessage/SendMessageCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Application.Features.Messaging.Commands.SendMessage;

// Mesaj gönderme işleyicisi + SignalR bildirimi + kalıcı Notification kaydı
public class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IChatNotifier _chatNotifier;
    private readonly INotificationService _notifications;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IChatNotifier chatNotifier,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _chatNotifier = chatNotifier;
        _notifications = notifications;
    }

    public async Task<Result<MessageDto>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<MessageDto>.Fail("Yetkisiz erişim.");

        var conversation = await _context.Conversations
            .Include(c => c.Participant1)
            .Include(c => c.Participant2)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null)
            return Result<MessageDto>.Fail("Conversation bulunamadı.");

        // Katılımcı doğrulama
        if (conversation.Participant1Id != userId.Value &&
            conversation.Participant2Id != userId.Value)
            return Result<MessageDto>.Fail("Bu conversation'a mesaj gönderme yetkiniz yok.");

        var sender = conversation.Participant1Id == userId.Value
            ? conversation.Participant1
            : conversation.Participant2;

        var recipientUserId = conversation.Participant1Id == userId.Value
            ? conversation.Participant2Id
            : conversation.Participant1Id;

        var content = request.Content.Trim();

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId.Value,
            Content = content,
            IsRead = false
        };

        _context.Messages.Add(message);

        // Conversation önizlemesini güncelle
        conversation.LastMessageAt = message.CreatedAt;
        conversation.LastMessagePreview = content.Length > 200 ? content[..200] : content;
        conversation.LastMessageSenderId = userId.Value;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender.FirstName,
            sender.LastName,
            sender.ProfileImageUrl,
            message.Content,
            message.IsRead,
            message.ReadAt,
            message.CreatedAt);

        // SignalR bildirimi (hata olsa bile mesaj kaydedildi, bildirimin patlaması akışı kırmasın)
        try
        {
            await _chatNotifier.NotifyMessageAsync(recipientUserId, dto, cancellationToken);
        }
        catch
        {
            // Sessiz geç: real-time bildirimin başarısızlığı persist edilen mesajı etkilemez
        }

        // Kalıcı bildirim kaydı (Notifications tablosu + ReceiveNotification push)
        try
        {
            var senderFullName = $"{sender.FirstName} {sender.LastName}".Trim();
            var preview = content.Length > 120 ? content[..120] + "..." : content;

            await _notifications.CreateAndPushAsync(
                recipientUserId: recipientUserId,
                type: NotificationType.NewMessage,
                title: $"{senderFullName} size mesaj gönderdi",
                body: preview,
                referenceType: "Message",
                referenceId: message.Id,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Sessiz geç: Notification kaydı patlarsa mesaj akışı etkilenmesin
        }

        return Result<MessageDto>.Ok(dto, "Mesaj gönderildi.");
    }
}