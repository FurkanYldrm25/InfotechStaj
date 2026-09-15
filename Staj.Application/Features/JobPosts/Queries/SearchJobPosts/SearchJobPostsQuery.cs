// Staj.Application/Features/JobPosts/Queries/SearchJobPosts/SearchJobPostsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Queries.SearchJobPosts;

// İş ilanı arama ve filtreleme sorgusu (yalnızca yayındakiler)
public record SearchJobPostsQuery(
    string? Keyword,
    Guid? CategoryId,
    List<Guid>? SkillIds,
    decimal? MinBudget,
    decimal? MaxBudget,
    WorkMode? WorkMode,
    string? Country,
    string? City,
    DateTime? PublishedAfter,
    string? SortBy,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<JobPostListItemDto>>>;