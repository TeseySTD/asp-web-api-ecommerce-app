using System;

namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record OrderItemId
{
    private OrderItemId(Guid orderItemId)
    {
        Value = orderItemId;
    }

    public Guid Value { get; set; }

    public static OrderItemId Create(Guid orderItemId)
    {
        return new(orderItemId);
    }
}

