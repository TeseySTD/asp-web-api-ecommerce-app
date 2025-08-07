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
        return Result<UserId>.Try(new UserId(productId))
            .Check(productId == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError() : Error("User Id cannot be empty", "UserId value object failure");
}