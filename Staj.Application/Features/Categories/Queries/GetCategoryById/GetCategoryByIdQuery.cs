// Staj.Application/Features/Categories/Queries/GetCategoryById/GetCategoryByIdQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Categories.Dtos;

namespace Staj.Application.Features.Categories.Queries.GetCategoryById;

// Belirli bir kategoriyi id ile getiren sorgu
public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDetailDto>>;