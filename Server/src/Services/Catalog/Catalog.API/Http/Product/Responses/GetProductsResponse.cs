using Catalog.Application.Dto.Product;
using Shared.Core.API;

namespace Catalog.API.Http.Product.Responses;

public sealed record GetProductsResponse(PaginatedResult<ProductReadDto> Products);