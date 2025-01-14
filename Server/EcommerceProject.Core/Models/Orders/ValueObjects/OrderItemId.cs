using System;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record OrderItemId
{
    private OrderItemId(Guid orderItemId)
    {
        Value = orderItemId;
    }

    public Guid Value { get; set; }

    public static Result<OrderItemId> Create(Guid orderItemId)
    {
        if (orderItemId == Guid.Empty)
            return new Error("OrderItemId is required","OrderItemId cannot be empty");
        return new OrderItemId(orderItemId);
    }
}

