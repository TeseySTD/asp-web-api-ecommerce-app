namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record OrderItemQuantity
{
    public uint Value { get; init; }

    private OrderItemQuantity(uint quantity)
    {
        Value = quantity;
    }

    public static OrderItemQuantity Create(uint quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan<uint>(1, quantity, nameof(quantity));
        return new OrderItemQuantity(quantity);
    }
    
}