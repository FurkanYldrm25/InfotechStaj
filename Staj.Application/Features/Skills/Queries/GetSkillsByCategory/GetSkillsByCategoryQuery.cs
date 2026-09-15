// Staj.Application/Features/Skills/Queries/GetSkillsByCategory/GetSkillsByCategoryQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetSkillsByCategory;

// Belirli bir kategoriye ait skill'leri getiren sorgu
public record GetSkillsByCategoryQuery(Guid CategoryId) : IRequest<Result<List<SkillDto>>>;