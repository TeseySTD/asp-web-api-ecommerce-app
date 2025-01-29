using Catalog.Application.Dto.Category;
using Shared.Core.API;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategories;

public record GetCategoriesQuery(PaginationRequest PaginationRequest) : IQuery<PaginatedResult<CategoryReadDto>>;