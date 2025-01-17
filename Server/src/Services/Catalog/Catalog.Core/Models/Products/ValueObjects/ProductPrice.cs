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
        var result = Result<ProductPrice>.Try()
            .Check(price < MinPrice || price > MaxPrice,
                new Error("Price is out of range", $"Price must be between {MinPrice} and {MaxPrice}"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductPrice(price);
    }

}
