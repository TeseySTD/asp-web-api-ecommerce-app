using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record ProductId
{
    public Guid Value { get; init; }

    private ProductId(Guid value)
    {
        Value = value;
    }
    public static Result<ProductId> Create(Guid productId)
    {
        return Result<ProductId>.Try(new ProductId(productId))
            .Check(productId == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError() : Error("Product Id cannot be empty", "ProductId value object failure");
}

