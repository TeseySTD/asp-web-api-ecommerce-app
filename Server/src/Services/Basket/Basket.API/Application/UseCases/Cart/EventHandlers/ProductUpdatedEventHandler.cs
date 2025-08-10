using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Basket.API.Application.UseCases.Cart.EventHandlers;

public class ProductUpdatedEventHandler : IntegrationEventHandler<ProductUpdatedEvent>
{
    private readonly ICartRepository _cartRepository;

    public ProductUpdatedEventHandler(
        ILogger<IntegrationEventHandler<ProductUpdatedEvent>> logger,
        ICartRepository cartRepository) : base(logger)
    {
        _cartRepository = cartRepository;
    }

    public override async Task Handle(ConsumeContext<ProductUpdatedEvent> context)
    {
        var productToUpdateId = ProductId.Create(context.Message.ProductId).Value;
        var result = await _cartRepository.GetCartsByProductId(productToUpdateId, context.CancellationToken);
        
        if (result.IsFailure)
        {
            Logger.LogInformation("Carts not found");
            return;
        }
        
        var carts = result.Value;
        var productToUpdate = context.Message;
        
        foreach (var cart in carts)
        {
            foreach (var item in cart.Items.Where(i => i.Id == productToUpdateId))
            {
                item.Update(
                    title: ProductTitle.Create(productToUpdate.Title).Value,
                    price: ProductPrice.Create(productToUpdate.Price).Value,
                    imageUrls: productToUpdate.ImageUrls,
                    category: productToUpdate.Category == null
                        ? null
                        : ProductCartItemCategory.Create(
                            CategoryId.Create(productToUpdate.Category.CategoryId).Value,
                            CategoryName.Create(productToUpdate.Category.Title).Value
                        )
                );
            }
        
            await _cartRepository.SaveCart(cart);
        }
    }
}