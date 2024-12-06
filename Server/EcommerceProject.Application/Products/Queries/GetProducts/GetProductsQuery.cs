using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Products;

namespace EcommerceProject.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IQuery<GetProductsResponse>;

public sealed record GetProductsResponse(IReadOnlyList<ProductDto> Products);