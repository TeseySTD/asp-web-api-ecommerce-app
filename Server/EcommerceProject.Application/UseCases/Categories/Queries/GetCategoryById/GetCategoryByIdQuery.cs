using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(CategoryId Id) : IQuery<CategoryDto>;