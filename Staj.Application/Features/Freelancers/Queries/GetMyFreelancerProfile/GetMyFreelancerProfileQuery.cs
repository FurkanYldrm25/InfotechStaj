// Staj.Application/Features/Freelancers/Queries/GetMyFreelancerProfile/GetMyFreelancerProfileQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Freelancers.Dtos;

namespace Staj.Application.Features.Freelancers.Queries.GetMyFreelancerProfile;

// Aktif kullanıcının freelancer profilini getirir
public record GetMyFreelancerProfileQuery() : IRequest<Result<FreelancerProfileDto>>;