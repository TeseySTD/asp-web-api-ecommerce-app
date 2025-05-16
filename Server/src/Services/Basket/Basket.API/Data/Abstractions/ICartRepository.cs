using Basket.API.Models;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Validation.Result;

namespace Basket.API.Data.Abstractions;

public interface ICartRepository
{
    Task<Result<ProductCart>> GetCartByUserId(UserId userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProductCart>>>GetCartsByProductId(ProductId productId, CancellationToken cancellationToken = default);
    Task<Result<ProductCart>> SaveCart(ProductCart cart, CancellationToken cancellationToken = default);
    Task<Result> DeleteCart(UserId userId, CancellationToken cancellationToken = default);
    Task <Result> StoreProductInCart(UserId userId, ProductCartItem item, CancellationToken cancellationToken = default);
    Task <Result> RemoveProductFromCart(UserId userId, ProductId productId, CancellationToken cancellationToken = default);
}