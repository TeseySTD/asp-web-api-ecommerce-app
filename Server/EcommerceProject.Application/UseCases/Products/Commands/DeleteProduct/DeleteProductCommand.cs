using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.UseCases.Products.Commands.DeleteProduct;

public record DeleteProductCommand(ProductId ProductId) : ICommand;