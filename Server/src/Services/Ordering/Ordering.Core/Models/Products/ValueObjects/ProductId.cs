using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Products.ValueObjects;

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
                    new ProductIdRequiredError()) 
                .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductId(productId);
    }

    public sealed record ProductIdRequiredError()
        : Error("Product Id cannot be empty", "ProductId value object failure");
}

