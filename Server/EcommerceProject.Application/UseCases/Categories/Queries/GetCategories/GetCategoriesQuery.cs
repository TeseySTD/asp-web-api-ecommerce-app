using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Categories;

namespace EcommerceProject.Application.UseCases.Categories.Queries.GetCategories;

public record GetCategoriesQuery() : IQuery<List<CategoryDto>>;