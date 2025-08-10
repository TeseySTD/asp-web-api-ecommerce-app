using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Mapster;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.StoreProduct;

public class StoreProductCommandHandler : ICommandHandler<StoreProductCommand>
{
    private readonly ICartRepository _cartRepository;

    public StoreProductCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<Result> Handle(StoreProductCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        var productItem = request.Product.Adapt<ProductCartItem>();
        return await _cartRepository.StoreProductInCart(userId, productItem, cancellationToken);
    }
}