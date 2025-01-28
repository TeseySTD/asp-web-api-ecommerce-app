using Catalog.Application.Dto.Category;

namespace Catalog.API.Http.Category.Responses;

public record GetCategoriesResponse(IEnumerable<CategoryReadDto> Categories);