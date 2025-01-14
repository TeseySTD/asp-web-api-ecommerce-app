using Shared.Core.Validation;

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
        var result = Result<ProductId>.TryFail()
                .CheckError(productId == Guid.Empty,
                    new Error("Product Id cannot be empty", "ProductId value object failure"))
                .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductId(productId);
    }
}

