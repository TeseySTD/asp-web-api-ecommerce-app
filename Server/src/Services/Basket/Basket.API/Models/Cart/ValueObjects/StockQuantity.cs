using System.Text.Json.Serialization;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record StockQuantity
{
    [JsonInclude] public uint Value { get; private set; }

    // For Marten
    [JsonConstructor]
    private StockQuantity()
    {
    }

    private StockQuantity(uint stockQuantity)
    {
        Value = stockQuantity;
    }

    public static Result<StockQuantity> Create(uint stockQuantity)
    {
        return Result<StockQuantity>.Try(new StockQuantity(stockQuantity))
            .Check(stockQuantity == 0, new QuantityLesserThanOneError())
            .Build(); 
    }

    public sealed record QuantityLesserThanOneError() : Error("Quantity must be greater than zero",
        "Quantity must be greater than zero because cart item cannot has zero quantity.");
}