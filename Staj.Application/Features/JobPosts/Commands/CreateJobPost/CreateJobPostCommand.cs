// Staj.Application/Features/JobPosts/Commands/CreateJobPost/CreateJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.CreateJobPost;

// Yeni iş ilanı oluşturma komutu (Client)
public record CreateJobPostCommand(
    Guid CategoryId,
    string Title,
    string Description,
    decimal? BudgetMin,
    decimal? BudgetMax,
    string? Currency,
    WorkMode WorkMode,
    int? DurationDays,
    string? Country,
    string? City,
    List<Guid> SkillIds
) : IRequest<Result<Guid>>;