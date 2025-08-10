using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Commands.SaveCart;

public record SaveCartCommand(ProductCartDto Dto) : ICommand;