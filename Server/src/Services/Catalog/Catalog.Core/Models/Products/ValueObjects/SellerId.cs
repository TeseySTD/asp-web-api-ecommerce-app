using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record SellerId
{
    public Guid Value { get; init; }

    private SellerId(Guid value)
    {
        Value = value;
    }

    public static Result<SellerId> Create(Guid sellerId)
    {
        return Result<SellerId>.Try(new SellerId(sellerId))
            .Check(sellerId == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError() : Error("Seller Id cannot be empty", "SellerId value object failure");
}