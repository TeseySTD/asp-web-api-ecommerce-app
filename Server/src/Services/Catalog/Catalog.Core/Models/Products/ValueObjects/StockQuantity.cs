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
         return Result<StockQuantity>.Try(new StockQuantity((uint) stockQuantity))
             .Check(stockQuantity < 0, new QuantityLesserThanZeroError())
             .Build(); 
    }
    
    public sealed record QuantityLesserThanZeroError() : Error("Quantity must be greater.", "Quantity must be greater than -1.");
}