// Staj.Application/Features/Categories/Queries/GetAllCategories/GetAllCategoriesQuery.cs
using MediatR;
using Staj.Application.Common.Models;
using Staj.Application.Features.Categories.Dtos;

namespace Staj.Application.Features.Categories.Queries.GetAllCategories;

// Tüm kategorileri getiren sorgu
public record GetAllCategoriesQuery(bool OnlyActive = true)
    : IRequest<Result<List<CategoryDto>>>;