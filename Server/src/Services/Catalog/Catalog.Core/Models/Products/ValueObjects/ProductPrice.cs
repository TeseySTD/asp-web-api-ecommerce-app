using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record ProductPrice
{
    public const decimal MinPrice = 0.1m;
    public const decimal MaxPrice = 1000000.0m;
    public decimal Value { get; init; }
    protected ProductPrice(decimal price)
    {
        Value = price;
    }

    public static Result<ProductPrice> Create(decimal price){
        return Result<ProductPrice>.Try(new ProductPrice(price))
            .Check(price < MinPrice || price > MaxPrice, new OutOfRangeError())
            .Build();
    }
    
    // For EF Core queries
    public static explicit operator decimal(ProductPrice price) => price.Value;

    public sealed record OutOfRangeError()
        : Error("Price is out of range", $"Price must be between {MinPrice} and {MaxPrice}");
}
