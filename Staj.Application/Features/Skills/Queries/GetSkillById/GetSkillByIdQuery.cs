// Staj.Application/Features/Skills/Queries/GetSkillById/GetSkillByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetSkillById;

// Belirli bir skill'i getiren sorgu
public record GetSkillByIdQuery(Guid Id) : IRequest<Result<SkillDetailDto>>;