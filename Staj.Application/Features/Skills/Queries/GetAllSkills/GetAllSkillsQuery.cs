// Staj.Application/Features/Skills/Queries/GetAllSkills/GetAllSkillsQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Skills.Dtos;

namespace Staj.Application.Features.Skills.Queries.GetAllSkills;

// Tüm skill'leri getiren sorgu
public record GetAllSkillsQuery(bool OnlyActive = true) : IRequest<Result<List<SkillDto>>>;