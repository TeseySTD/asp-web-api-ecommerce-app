using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.DeleteCart;

public class DeleteCartCommandHandler : ICommandHandler<DeleteCartCommand>
{
    private readonly ICartRepository _cartRepository;

    public DeleteCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<Result> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        return await _cartRepository.DeleteCart(userId, cancellationToken);
    }
}