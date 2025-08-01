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
        if (stockQuantity == 0) return new QuantityLesserThanOneError();
        return new StockQuantity(stockQuantity);
    }

    public sealed record QuantityLesserThanOneError() : Error("Quantity must be greater than zero",
        "Quantity must be greater than zero because cart item cannot has zero quantity.");
}