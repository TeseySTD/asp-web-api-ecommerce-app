using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Orders.ValueObjects;

public record CustomerId
{
    private CustomerId(Guid userId)
    {
        Value = userId;
    }

    public Guid Value { get; set; }

    public static Result<CustomerId> Create(Guid value)
    {
        if(value == Guid.Empty)
            return new Error("User id cannot be empty", nameof(value));
        
        return new CustomerId(value);
    }
}