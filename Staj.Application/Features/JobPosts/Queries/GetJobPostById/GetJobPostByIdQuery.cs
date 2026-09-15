// Staj.Application/Features/JobPosts/Queries/GetJobPostById/GetJobPostByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.JobPosts.Dtos;

namespace Staj.Application.Features.JobPosts.Queries.GetJobPostById;

// İş ilanı detayını getiren sorgu
public record GetJobPostByIdQuery(Guid Id) : IRequest<Result<JobPostDto>>;