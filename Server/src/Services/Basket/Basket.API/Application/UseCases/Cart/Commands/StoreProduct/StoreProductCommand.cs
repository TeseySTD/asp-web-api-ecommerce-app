using Basket.API.Dto.Cart;
using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Commands.StoreProduct;

public record StoreProductCommand(Guid UserId, ProductCartItemDto Product) : ICommand;