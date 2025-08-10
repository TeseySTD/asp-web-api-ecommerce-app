using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;

public record IncreaseQuantityCommand(Guid Id, Guid SellerId, int Quantity) : ICommand;