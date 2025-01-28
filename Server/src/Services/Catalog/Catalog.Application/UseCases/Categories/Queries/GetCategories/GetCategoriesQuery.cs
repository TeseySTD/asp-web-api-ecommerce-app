using Catalog.Application.Dto.Category;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategories;

public record GetCategoriesQuery() : IQuery<List<CategoryReadDto>>;