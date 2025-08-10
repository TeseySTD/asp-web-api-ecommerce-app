using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Basket.API.Application.UseCases.Cart.EventHandlers;

public class ProductDeletedEventHandler : IntegrationEventHandler<ProductDeletedEvent>
{
    private readonly ICartRepository _cartRepository;

    public ProductDeletedEventHandler(ILogger<IntegrationEventHandler<ProductDeletedEvent>> logger,
        ICartRepository cartRepository) : base(logger)
    {
        _cartRepository = cartRepository;
    }

    public override async Task Handle(ConsumeContext<ProductDeletedEvent> context)
    {
        var productToDeleteId = ProductId.From(context.Message.ProductId);

        var getCartsResult = await _cartRepository.GetCartsByProductId(productToDeleteId);

        if (getCartsResult.IsFailure)
        {
            Logger.LogInformation("Carts not found");
            return;
        }

        var carts = getCartsResult.Value;

        foreach (var cart in carts)
        {
            cart.RemoveItem(productToDeleteId);

            var saveResult = await _cartRepository.SaveCart(cart);

            if (saveResult.IsFailure)
            {
                Logger.LogError("Failed to save cart {CartId} after product delete: {Errors}",
                    cart.Id.Value, saveResult.Errors.Aggregate("", (s, e) => s + $"{e.Message}:{e.Description}, "));
            }
            else
            {
                Logger.LogInformation("Successfully updated cart {CartId} with product information",
                    cart.Id.Value);
            }
        }
    }
}