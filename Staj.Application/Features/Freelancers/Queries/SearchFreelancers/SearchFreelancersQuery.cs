// Staj.Application/Features/Freelancers/Queries/SearchFreelancers/SearchFreelancersQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;

namespace Staj.Application.Features.Freelancers.Queries.SearchFreelancers;

// Freelancer arama ve filtreleme sorgusu
public record SearchFreelancersQuery(
    string? Keyword,
    Guid? CategoryId,
    List<Guid>? SkillIds,
    decimal? MinHourlyRate,
    decimal? MaxHourlyRate,
    string? Country,
    string? City,
    bool? IsAvailable,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<FreelancerListItemDto>>>;