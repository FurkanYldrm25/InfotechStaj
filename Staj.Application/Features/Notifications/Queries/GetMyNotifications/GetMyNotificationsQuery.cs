// Staj.Application/Features/Notifications/Queries/GetMyNotifications/GetMyNotificationsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Notifications.Dtos;

namespace Staj.Application.Features.Notifications.Queries.GetMyNotifications;

// Kullanıcının bildirimlerini sayfalı getirir
public record GetMyNotificationsQuery(
    bool? OnlyUnread,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<NotificationDto>>>;