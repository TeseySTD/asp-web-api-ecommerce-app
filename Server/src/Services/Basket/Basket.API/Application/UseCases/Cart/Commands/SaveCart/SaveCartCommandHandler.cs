using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Mapster;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.SaveCart;

public class SaveCartCommandHandler : ICommandHandler<SaveCartCommand>
{
    private readonly ICartRepository _cartRepository;

    public SaveCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<Result> Handle(SaveCartCommand request, CancellationToken cancellationToken)
    {
        var cart = request.Dto.Adapt<ProductCart>();
        return await _cartRepository.SaveCart(cart, cancellationToken);
    }
}