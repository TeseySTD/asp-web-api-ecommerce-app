using Shared.Core.Validation;
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
            return new QuantityLessThenZeroError(); 
        return new OrderItemQuantity(quantity);
    }
    
    public sealed record QuantityLessThenZeroError() : Error(nameof(OrderItemQuantity), $"Quantity must be greater than zero");
}