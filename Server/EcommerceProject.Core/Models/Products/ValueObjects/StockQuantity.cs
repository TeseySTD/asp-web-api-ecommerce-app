namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record StockQuantity
{
    public uint Value { get; init; }

    private StockQuantity(uint stockQuantity)
    {
        Value = stockQuantity;
    }

    public static StockQuantity Create(uint stockQuantity)
    {
        return new StockQuantity(stockQuantity);
    }
}