using Microsoft.IdentityModel.Tokens;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record StockQuantity
{
    public uint Value { get; init; }

    private StockQuantity(uint stockQuantity)
    {
        Value = stockQuantity;
    }

    public static Result<StockQuantity> Create(uint stockQuantity) => new StockQuantity(stockQuantity);
    
    public static Result<StockQuantity> Create(int stockQuantity)
    {
        if (stockQuantity < 0) return new QuantityLesserThanZeroError();
        return Create((uint)stockQuantity);
    }
    
    public sealed record QuantityLesserThanZeroError() : Error("Quantity must be greater.", "Quantity must be greater than -1.");
}