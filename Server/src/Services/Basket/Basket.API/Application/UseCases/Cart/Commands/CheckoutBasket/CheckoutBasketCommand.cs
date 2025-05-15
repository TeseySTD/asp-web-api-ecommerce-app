using Basket.API.Dto.Cart;
using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Commands.CheckoutBasket;

public record CheckoutBasketCommand(CheckoutBasketDto Dto) : ICommand;