using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;

namespace EcommerceProject.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductDto dto) : ICommand;