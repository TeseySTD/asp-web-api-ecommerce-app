using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

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
                new Error("Product Id cannot be empty", "ProductId value object failure"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductId(productId);
    }
}