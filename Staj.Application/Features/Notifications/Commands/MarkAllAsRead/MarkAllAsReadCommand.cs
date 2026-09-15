// Staj.Application/Features/Notifications/Commands/MarkAllAsRead/MarkAllAsReadCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Notifications.Commands.MarkAllAsRead;

// Tüm bildirimleri okundu işaretle
public record MarkAllAsReadCommand : IRequest<Result>;