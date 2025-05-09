using Basket.API.Data.Abstractions;
using Basket.API.Models;
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

    public async Task<Result<ProductCart>> GetCart(UserId userId, CancellationToken cancellationToken = default)
    {
        var cart = await _session.LoadAsync<ProductCart>(userId, cancellationToken);
        return cart is not null
            ? Result<ProductCart>.Success(cart)
            : new Error("Cart not found", $"Cart for user with id {userId} not found");
    }

    public async Task<Result<ProductCart>> StoreCart(ProductCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            if(await _session.LoadAsync<ProductCart>(cart.Id, cancellationToken) is not null)
                return new Error("Cart already exists", $"User with id {cart.Id.Value} already has a cart");
            
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
            if(await _session.LoadAsync<ProductCart>(userId, cancellationToken) is null)
                return new Error("Cart not found", $"Cart with user id {userId.Value} not found");
            
            _session.Delete<ProductCart>(userId);
            await _session.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new Error("Cart not deleted", e.Message);
        }
        return Result.Success();
    }

    public async Task<Result<ProductCart>> UpdateCart(ProductCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            if(await _session.LoadAsync<ProductCart>(cart.Id, cancellationToken) is null)
                return new Error("Cart not found", $"Cart with user id {cart.Id.Value} not found");
            
            _session.Update(cart);
            await _session.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new Error("Cart was not updated", e.Message);
        }
        return Result<ProductCart>.Success(cart);
    }

    public async Task<Result> StoreProductInCart(UserId userId, ProductCartItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _session.LoadAsync<ProductCart>(userId, cancellationToken);
            if(cart is null)
                return new Error("Cart not found", $"Cart with user id {userId.Value} not found");
            else if (cart.HasItem(item.Id))
                return new Error("Product already in cart", $"Product with id {item.Id.Value} already in cart");
            
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
            if(cart is null)
                return new Error("Cart not found", $"Cart with user id {userId.Value} not found");
            else if (!cart.HasItem(productId))
                return new Error("Product not in cart", $"Product with id {productId.Value} not in cart");
            
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