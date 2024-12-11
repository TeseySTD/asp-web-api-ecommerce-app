using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(ProductDto Value) : ICommand;


