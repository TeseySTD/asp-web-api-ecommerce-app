using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(ProductId Id) : IQuery<GetProductByIdResponse>;
