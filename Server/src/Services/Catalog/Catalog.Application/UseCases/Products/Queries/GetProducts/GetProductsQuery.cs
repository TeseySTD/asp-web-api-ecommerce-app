using Catalog.Application.Dto.Product;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IQuery<IReadOnlyList<ProductReadDto>>;

