using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Commands.RemoveProduct;

public record RemoveProductCommand(Guid UserId, Guid ProductId) : ICommand;