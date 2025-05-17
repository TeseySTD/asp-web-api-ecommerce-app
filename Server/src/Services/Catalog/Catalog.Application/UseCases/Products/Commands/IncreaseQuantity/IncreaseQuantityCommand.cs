using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;

public record IncreaseQuantityCommand(Guid Id, int Quantity) : ICommand;