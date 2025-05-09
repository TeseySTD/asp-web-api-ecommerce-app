using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public struct UserId
{
    public Guid Value { get; init; }

    private UserId(Guid value)
    {
        Value = value;
    }

    // For Marten
    public static UserId From(Guid value) => new(value);
    
    public static Result<UserId> Create(Guid productId)
    {
        var result = Result<UserId>.Try()
            .Check(productId == Guid.Empty,
                new Error("User Id cannot be empty", "UserId value object failure"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new UserId(productId);
    }
}