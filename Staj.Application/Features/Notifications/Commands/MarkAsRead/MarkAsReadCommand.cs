// Staj.Application/Features/Notifications/Commands/MarkAsRead/MarkAsReadCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.MarkAsRead;

// Tek bir bildirimi okundu işaretle
public record MarkAsReadCommand(Guid Id) : IRequest<Result>;