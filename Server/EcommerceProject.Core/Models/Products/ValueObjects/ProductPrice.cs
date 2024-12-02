namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record ProductPrice
{
    public const decimal MinPrice = 0.1m;
    public const decimal MaxPrice = 1000000.0m;
    public decimal Value { get; init; }
    protected ProductPrice(decimal price)
    {
        Value = price;
    }

    public static ProductPrice Create(decimal price){
        if(price < MinPrice || price > MaxPrice) 
            throw new Exception($"Price must be between {MinPrice} and {MaxPrice}");
        return new ProductPrice(price);
    }

}
