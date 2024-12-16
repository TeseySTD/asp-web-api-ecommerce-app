using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.API.Http.Category.Responses;

public record GetCategoriesResponse(IEnumerable<CategoryDto> Categories);