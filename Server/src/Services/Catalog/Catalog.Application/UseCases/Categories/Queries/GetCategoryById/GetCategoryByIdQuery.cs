using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(CategoryId Id) : IQuery<CategoryDto>;