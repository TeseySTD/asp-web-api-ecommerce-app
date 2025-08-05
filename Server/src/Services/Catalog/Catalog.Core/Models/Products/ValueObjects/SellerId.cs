using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record SellerId
{
    public Guid Value { get; init; }

    private SellerId(Guid value)
    {
        Value = value;
    }

    public static Result<SellerId> Create(Guid productId)
    {
        var result = Result<SellerId>.Try()
            .Check(productId == Guid.Empty,
                new IdRequiredError())
            .Build();

        if (result.IsFailure)
            return result;
        return new SellerId(productId);
    }

    public sealed record IdRequiredError() : Error("Product Id cannot be empty", "ProductId value object failure");
}