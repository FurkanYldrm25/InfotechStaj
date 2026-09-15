// Staj.Application/Features/JobPosts/Commands/CloseJobPost/CloseJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.JobPosts.Commands.CloseJobPost;

// İş ilanını kapatma komutu (iş tamamlandı/uygun aday bulundu)
public record CloseJobPostCommand(Guid Id) : IRequest<Result>;