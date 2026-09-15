// Staj.Application/Features/JobPosts/Commands/UpdateJobPost/UpdateJobPostCommand.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Commands.UpdateJobPost;

// İş ilanı güncelleme komutu (sadece sahibi Client)
public record UpdateJobPostCommand(
    Guid Id,
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
) : IRequest<Result>;