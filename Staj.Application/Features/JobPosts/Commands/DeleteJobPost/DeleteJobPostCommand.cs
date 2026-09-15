// Staj.Application/Features/JobPosts/Commands/DeleteJobPost/DeleteJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.JobPosts.Commands.DeleteJobPost;

// İş ilanını silme komutu (soft delete)
public record DeleteJobPostCommand(Guid Id) : IRequest<Result>;