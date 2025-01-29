using Catalog.Application.Dto.Product;
using Shared.Core.API;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public sealed record GetProductsQuery(PaginationRequest PaginationRequest)
    : IQuery<PaginatedResult<ProductReadDto>>;