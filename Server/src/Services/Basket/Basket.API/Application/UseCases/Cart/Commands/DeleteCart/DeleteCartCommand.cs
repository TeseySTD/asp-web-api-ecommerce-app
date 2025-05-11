using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Commands.DeleteCart;

public record DeleteCartCommand(Guid UserId) : ICommand;