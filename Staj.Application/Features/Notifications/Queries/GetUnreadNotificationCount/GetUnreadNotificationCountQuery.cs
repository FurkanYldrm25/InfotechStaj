// Staj.Application/Features/Notifications/Queries/GetUnreadNotificationCount/GetUnreadNotificationCountQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Notifications.Dtos;

namespace Staj.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

// Okunmamış bildirim sayacı
public record GetUnreadNotificationCountQuery : IRequest<Result<UnreadCountDto>>;