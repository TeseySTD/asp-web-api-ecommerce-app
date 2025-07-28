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
        var result = Result<ProductId>.Try()
                .Check(productId == Guid.Empty,
                    new IdRequiredError())
                .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductId(productId);
    }

    public sealed record IdRequiredError() : Error("Product Id cannot be empty", "ProductId value object failure");
}

