using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IQuery<IReadOnlyList<ProductReadDto>>;

