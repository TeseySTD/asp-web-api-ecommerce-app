using Catalog.Application.Dto.Category;
using Shared.Core.API;

namespace Catalog.API.Http.Category.Responses;

public record GetCategoriesResponse(PaginatedResult<CategoryReadDto> Categories);