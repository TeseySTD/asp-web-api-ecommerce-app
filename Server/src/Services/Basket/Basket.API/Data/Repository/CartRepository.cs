using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Marten;
using Shared.Core.Validation.Result;

namespace Basket.API.Data.Repository;

public class CartRepository : ICartRepository
{
    private readonly IDocumentSession _session;

    public CartRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<Result<ProductCart>> GetCartByUserId(UserId userId, CancellationToken cancellationToken = default)
    {
        var cart = await _session.LoadAsync<ProductCart>(userId, cancellationToken);
        return cart is not null
            ? Result<ProductCart>.Success(cart)
            : new ICartRepository.CartWithUserIdNotFoundError(userId.Value);
    }

    public async Task<Result<IEnumerable<ProductCart>>> GetCartsByProductId(ProductId productId,
        CancellationToken cancellationToken = default)
    {
        var carts = await _session
            .Query<ProductCart>()
            .Where(c => c.Items.Any(i => i.Id.Value == productId.Value))
            .ToListAsync();
        return !carts.IsEmpty()
            ? Result<IEnumerable<ProductCart>>.Success(carts)
            : new ICartRepository.CartWithProductIdNotFoundError(productId.Value);
    }

    public async Task<Result<ProductCart>> SaveCart(ProductCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            _session.Store(cart);

            await _session.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new Error("Cart not stored", e.Message);
        }

        return cart;
    }

    public async Task<Result> DeleteCart(UserId userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _session.LoadAsync<ProductCart>(userId, cancellationToken) is null)
                return new ICartRepository.CartWithUserIdNotFoundError(userId.Value);

            _session.Delete<ProductCart>(userId);
            await _session.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new Error("Cart not deleted", e.Message);
        }

        return Result.Success();
    }

    public async Task<Result> StoreProductInCart(UserId userId, ProductCartItem item,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _session.LoadAsync<ProductCart>(userId, cancellationToken);

            if (cart is null)
            {
                cart = ProductCart.Create(userId);
                cart.AddItem(item);
                _session.Store(cart);
                await _session.SaveChangesAsync();
                return Result.Success();
            }
            else if (cart.HasItem(item.Id))
                return new ICartRepository.ProductAlreadyInCartError(item.Id.Value);

            cart.AddItem(item);
            _session.Update(cart);
            await _session.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error($"Product with id {item.Id.Value} not stored in cart", e.Message);
        }
    }

    public async Task<Result> RemoveProductFromCart(UserId userId, ProductId productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _session.LoadAsync<ProductCart>(userId, cancellationToken);
            if (cart is null)
                return new ICartRepository.CartWithUserIdNotFoundError(userId.Value);
            else if (!cart.HasItem(productId))
                return new ICartRepository.ProductInCartNotFound(productId.Value);

            cart.RemoveItem(productId);
            _session.Update(cart);
            await _session.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error($"Product with id {productId.Value} was not removed from cart", e.Message);
        }
    }
}