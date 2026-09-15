// Staj.Application/Features/Notifications/Commands/DeleteNotification/DeleteNotificationCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.DeleteNotification;

// Bildirim silme (soft, sadece kendi bildirimi)
public record DeleteNotificationCommand(Guid Id) : IRequest<Result>;