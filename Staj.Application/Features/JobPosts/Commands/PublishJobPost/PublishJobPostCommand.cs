// Staj.Application/Features/JobPosts/Commands/PublishJobPost/PublishJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.JobPosts.Commands.PublishJobPost;

// İş ilanını yayına alma komutu
public record PublishJobPostCommand(Guid Id) : IRequest<Result>;