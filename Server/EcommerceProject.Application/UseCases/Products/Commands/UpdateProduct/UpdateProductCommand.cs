using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.Application.UseCases.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductDto Value) : ICommand;