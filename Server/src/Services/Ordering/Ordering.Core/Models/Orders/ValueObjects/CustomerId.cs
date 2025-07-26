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
        if (value == Guid.Empty)
            return new CustomerIdRequiredError(); 
        
        return new CustomerId(value);
    }
    
    public sealed record CustomerIdRequiredError() : Error("Customer id can not be empty", "Customer id must be provided");
}
