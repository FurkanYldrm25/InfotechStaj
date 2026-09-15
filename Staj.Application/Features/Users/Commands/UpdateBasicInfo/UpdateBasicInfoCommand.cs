// Staj.Application/Features/Users/Commands/UpdateBasicInfo/UpdateBasicInfoCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Users.Commands.UpdateBasicInfo;

// Ad-soyad gibi temel kullanıcı bilgilerini günceller
public record UpdateBasicInfoCommand(string FirstName, string LastName) : IRequest<Result>;