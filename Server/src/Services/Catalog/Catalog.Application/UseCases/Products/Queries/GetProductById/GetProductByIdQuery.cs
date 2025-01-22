using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Queries.GetProductById;

public record GetProductByIdQuery(ProductId Id) : IQuery<ProductReadDto>;
