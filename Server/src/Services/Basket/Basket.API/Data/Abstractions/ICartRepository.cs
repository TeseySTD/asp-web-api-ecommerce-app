using System.Linq.Expressions;
using Basket.API.Models;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Validation.Result;

namespace Basket.API.Data.Abstractions;

public interface ICartRepository
{
    Task<Result<ProductCart>> GetCartByUserId(UserId userId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<ProductCart>>> GetCartsByPredicate(Expression<Func<ProductCart, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<ProductCart>>> GetCartsByProductId(ProductId productId,
        CancellationToken cancellationToken = default);

    Task<Result<ProductCart>> SaveCart(ProductCart cart, CancellationToken cancellationToken = default);
    Task<Result> DeleteCart(UserId userId, CancellationToken cancellationToken = default);
    Task<Result> StoreProductInCart(UserId userId, ProductCartItem item, CancellationToken cancellationToken = default);

    Task<Result> RemoveProductFromCart(UserId userId, ProductId productId,
        CancellationToken cancellationToken = default);

    public sealed record PredicateExceptionError(string Message, string Description) : Error(Message, Description);
    public sealed record CartWithUserIdNotFoundError(Guid UserId)
        : Error("Cart not found", $"Cart for user with id {UserId} not found");

    public sealed record CartWithProductIdNotFoundError(Guid ProductId)
        : Error("Carts not found", $"Carts with product with id {ProductId} not found");

    public sealed record ProductAlreadyInCartError(Guid ProductId)
        : Error("Product already in cart", $"Product with id {ProductId} already in cart");

    public sealed record ProductInCartNotFound(Guid ProductId)
        : Error("Product not in cart", $"Product with id {ProductId} not in cart");
}