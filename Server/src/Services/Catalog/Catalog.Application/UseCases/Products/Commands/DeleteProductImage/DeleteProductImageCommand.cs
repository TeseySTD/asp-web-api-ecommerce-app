using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, Guid SellerId, Guid ImageId) : ICommand;