// Staj.Application/Features/Freelancers/Queries/GetFreelancerById/GetFreelancerByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;

namespace Staj.Application.Features.Freelancers.Queries.GetFreelancerById;

// Belirli bir freelancer profilini getirir (public)
public record GetFreelancerByIdQuery(Guid Id) : IRequest<Result<FreelancerProfileDto>>;