using System.Text.Json.Serialization;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record ProductId
{
    [JsonInclude]
    public Guid Value { get; private set; }
    
    // For Marten
    public static ProductId From(Guid value) => new(value);
    [JsonConstructor]
    private ProductId() { }
    
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