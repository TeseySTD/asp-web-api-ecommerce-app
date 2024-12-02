using System;

namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record OrderId
{
    private OrderId(Guid orderId)
    {
        Value = Guid.NewGuid();
    }

    public Guid Value { get; private set; }

    public static OrderId Create(Guid orderId)
    {
        return new OrderId(orderId);
    }
}
