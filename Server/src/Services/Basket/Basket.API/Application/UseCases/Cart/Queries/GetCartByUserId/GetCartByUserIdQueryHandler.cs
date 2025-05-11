using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Marten.Linq.QueryHandlers;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Queries.GetCartByUserId;

public class GetCartByUserIdQueryHandler : IQueryHandler<GetCartByUserIdQuery, ProductCart>
{
    private readonly ICartRepository _cartRepository;

    public GetCartByUserIdQueryHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public Task<Result<ProductCart>> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);
        return _cartRepository.GetCartByUserId(userId, cancellationToken);
    }
}