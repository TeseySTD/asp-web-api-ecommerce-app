using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;

public record DecreaseQuantityCommand(Guid Id, Guid SellerId, int Quantity) : ICommand;