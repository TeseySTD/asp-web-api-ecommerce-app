using System.Text.Json.Serialization;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record StockQuantity
{
    [JsonInclude]
    public uint Value { get; private set; }

    // For Marten
    [JsonConstructor]
    private StockQuantity() { }
    private StockQuantity(uint stockQuantity)
    {
        Value = stockQuantity;
    }

    public static Result<StockQuantity> Create(uint stockQuantity)
    {
        if (stockQuantity == 0) return new Error("Quantity must be greater than zero", nameof(stockQuantity));
        return new StockQuantity(stockQuantity);
    }
}