using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Category;

namespace Basket.API.Application.UseCases.Cart.EventHandlers;

public class CategoryDeletedEventHandler : IntegrationEventHandler<CategoryDeletedEvent>
{
    private readonly ICartRepository _cartRepository;

    public CategoryDeletedEventHandler(ILogger<IntegrationEventHandler<CategoryDeletedEvent>> logger,
        ICartRepository cartRepository) : base(logger)
    {
        _cartRepository = cartRepository;
    }

    public override async Task Handle(ConsumeContext<CategoryDeletedEvent> context)
    {
        var categoryToDeleteId = CategoryId.Create(context.Message.CategoryId).Value;
        var getCartsResult = await _cartRepository
            .GetCartsByPredicate(c => c.Items.Any(i => i.Category != null && i.Category.Id == categoryToDeleteId));

        if (getCartsResult.IsFailure)
        {
            Logger.LogError("Failed to get carts with category: {Error}", getCartsResult.Errors.First());
            return;
        }

        var carts = getCartsResult.Value;

        foreach (var cart in carts)
        {
            foreach (var item in cart.Items.Where(i => i.Category != null && i.Category.Id == categoryToDeleteId))
                item.Update(null);
            
            var saveResult = await _cartRepository.SaveCart(cart);

            if (saveResult.IsFailure)
            {
                Logger.LogError("Failed to save cart {CartId} after category update: {Errors}",
                    cart.Id.Value, saveResult.Errors.Aggregate("", (s, e) => s + $"{e.Message}:{e.Description}, "));
            }
            else
            {
                Logger.LogInformation("Successfully updated cart {CartId} with new category information",
                    cart.Id.Value);
            }
        }
    }
}