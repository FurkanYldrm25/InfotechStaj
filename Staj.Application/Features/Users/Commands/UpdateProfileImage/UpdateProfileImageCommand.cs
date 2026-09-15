// Staj.Application/Features/Users/Commands/UpdateProfileImage/UpdateProfileImageCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Users.Commands.UpdateProfileImage;

// Profil fotoğrafı URL'sini günceller
public record UpdateProfileImageCommand(string ImageUrl) : IRequest<Result>;