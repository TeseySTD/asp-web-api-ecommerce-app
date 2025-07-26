using Shared.Core.Validation;
using Shared.Core.Validation.Result;

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
            return new OrderItemIdRequiredError();
        return new OrderItemId(orderItemId);
    }
    
    public sealed record OrderItemIdRequiredError() : Error("OrderItemId is required", "OrderItemId cannot be empty");
}

