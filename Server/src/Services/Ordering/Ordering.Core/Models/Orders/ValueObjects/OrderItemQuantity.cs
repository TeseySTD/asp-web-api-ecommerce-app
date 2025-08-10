using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Orders.ValueObjects;

public record OrderItemQuantity
{
    public uint Value { get; init; }

    private OrderItemQuantity(uint quantity)
    {
        Value = quantity;
    }

    public static Result<OrderItemQuantity> Create(uint quantity)
    {
        if (quantity < 1)
            return new QuantityLessThenOneError(); 
        return new OrderItemQuantity(quantity);
    }
    
    public sealed record QuantityLessThenOneError() : Error(nameof(OrderItemQuantity), $"Quantity must be greater than one");
}