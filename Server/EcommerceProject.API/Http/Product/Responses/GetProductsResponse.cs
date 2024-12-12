using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.API.Http.Product.Responses;

public sealed record GetProductsResponse(IReadOnlyList<ProductReadDto> Products);