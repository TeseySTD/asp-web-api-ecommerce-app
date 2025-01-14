using Shared.Core.Validation;

namespace Ordering.Core.Models.Orders.ValueObjects;

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

