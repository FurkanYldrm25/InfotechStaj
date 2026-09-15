// Staj.Application/Features/Messaging/Queries/GetUnreadCount/GetUnreadCountQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Messaging.Dtos;

namespace Staj.Application.Features.Messaging.Queries.GetUnreadCount;

// Kullanıcının toplam okunmamış mesaj sayısını getirir
public record GetUnreadCountQuery() : IRequest<Result<UnreadCountDto>>;