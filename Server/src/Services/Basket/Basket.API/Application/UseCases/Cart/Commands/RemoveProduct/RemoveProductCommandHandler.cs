using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.RemoveProduct;

public class RemoveProductCommandHandler : ICommandHandler<RemoveProductCommand>
{
    private readonly ICartRepository _cartRepository;

    public RemoveProductCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<Result> Handle(RemoveProductCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        var productId = ProductId.From(request.ProductId);
        return await _cartRepository.RemoveProductFromCart(userId, productId, cancellationToken);
    }
}