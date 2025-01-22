using Catalog.Application.Dto.Product;

namespace Catalog.API.Http.Product.Responses;

public sealed record GetProductsResponse(IReadOnlyList<ProductReadDto> Products);