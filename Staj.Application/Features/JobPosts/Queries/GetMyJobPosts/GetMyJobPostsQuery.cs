// Staj.Application/Features/JobPosts/Queries/GetMyJobPosts/GetMyJobPostsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;
using Staj.Domain.Enums;

namespace Staj.Application.Features.JobPosts.Queries.GetMyJobPosts;

// Client'ın kendi ilanlarını getiren sorgu
public record GetMyJobPostsQuery(
    JobPostStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<JobPostListItemDto>>>;