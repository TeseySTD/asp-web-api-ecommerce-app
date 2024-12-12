using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProductById;

public record GetProductByIdQuery(ProductId Id) : IQuery<ProductReadDto>;
