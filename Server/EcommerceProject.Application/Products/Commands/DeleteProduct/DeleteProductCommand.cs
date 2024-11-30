using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;

namespace EcommerceProject.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(ProductId ProductId) : ICommand;