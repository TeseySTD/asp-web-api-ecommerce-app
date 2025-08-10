using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Category;

namespace Basket.API.Application.UseCases.Cart.EventHandlers;

public class CategoryUpdatedEventHandler : IntegrationEventHandler<CategoryUpdatedEvent>
{
    private readonly ICartRepository _cartRepository;

    public CategoryUpdatedEventHandler(ILogger<IntegrationEventHandler<CategoryUpdatedEvent>> logger,
        ICartRepository cartRepository) : base(logger)
    {
        _cartRepository = cartRepository;
    }

    public override async Task Handle(ConsumeContext<CategoryUpdatedEvent> context)
    {
        var @event = context.Message;
        var categoryId = CategoryId.Create(@event.CategoryId).Value;
        var categoryName = CategoryName.Create(@event.CategoryName).Value;

        var getCartsResult = await _cartRepository
            .GetCartsByPredicate(c => c.Items
                .Any(i => i.Category != null && i.Category.Id == categoryId));
        
        if (getCartsResult.IsFailure)
        {
            Logger.LogError("Failed to get carts with category: {Error}", getCartsResult.Errors.First());
            return;
        }
        
        var carts = getCartsResult.Value;
        foreach (var cart in carts)
        {
            var itemsToUpdate = cart.Items
                .Where(i => i.Category != null && i.Category.Id == categoryId)
                .ToList();

            foreach (var item in itemsToUpdate)
                item.Update(ProductCartItemCategory.Create(categoryId, categoryName));

            var saveResult = await _cartRepository.SaveCart(cart);

            if (saveResult.IsFailure)
            {
                Logger.LogError("Failed to save cart {CartId} after category update: {Errors}",
                    cart.Id.Value, saveResult.Errors.Aggregate("", (s, e) => s +$"{e.Message}:{e.Description}, "));
            }
            else
            {
                Logger.LogInformation("Successfully updated cart {CartId} with new category information",
                    cart.Id.Value);
            }
        }
    }
}