using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;

namespace EcommerceProject.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(ProductDto Value) : ICommand;


