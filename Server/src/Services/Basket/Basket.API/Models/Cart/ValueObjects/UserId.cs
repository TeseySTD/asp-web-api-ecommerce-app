using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public struct UserId
{
    public Guid Value { get; init; }

    // For Marten
    public static UserId From(Guid value) => new(value);
    
    private UserId(Guid value)
    {
        Value = value;
    }


    
    public static Result<UserId> Create(Guid productId)
    {
        var result = Result<UserId>.Try()
            .Check(productId == Guid.Empty,
                new IdRequiredError())
            .Build();
        
        if(result.IsFailure)
            return result;
        return new UserId(productId);
    }

    public sealed record IdRequiredError() : Error("User Id cannot be empty", "UserId value object failure");
}