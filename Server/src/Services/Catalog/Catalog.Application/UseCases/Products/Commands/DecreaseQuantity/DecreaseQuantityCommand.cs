using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;

public record DecreaseQuantityCommand(Guid Id, int Quantity) : ICommand;