// Staj.Application/Features/JobPosts/Commands/CancelJobPost/CancelJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.JobPosts.Commands.CancelJobPost;

// İş ilanını iptal etme komutu
public record CancelJobPostCommand(Guid Id) : IRequest<Result>;