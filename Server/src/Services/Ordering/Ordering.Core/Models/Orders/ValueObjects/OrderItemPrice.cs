using Shared.Core.Validation;

namespace Ordering.Core.Models.Orders.ValueObjects;

public record OrderItemPrice
{
    public const decimal MinPrice = 0.1m;
    public const decimal MaxPrice = 1000000.0m;
    public decimal Value { get; init; }
    protected OrderItemPrice(decimal price)
    {
        Value = price;
    }

    public static Result<OrderItemPrice> Create(decimal price){
        var result = Result<OrderItemPrice>.TryFail()
            .CheckError(price < MinPrice || price > MaxPrice,
                new Error("Price is out of range", $"Price must be between {MinPrice} and {MaxPrice}"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new OrderItemPrice(price);
    }

}
