using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Orders.ValueObjects;

public record OrderId
{
    private OrderId(Guid orderId)
    {
        Value = orderId;
    }

    public Guid Value { get; private set; }

    public static Result<OrderId> Create(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return new Error("Invalid order Id", "Order Id cannot be empty");
        return new OrderId(orderId);
    }
}
