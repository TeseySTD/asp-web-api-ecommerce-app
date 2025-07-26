using Shared.Core.Validation;
using Shared.Core.Validation.Result;

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
        var result = Result<OrderItemPrice>.Try()
            .Check(price < MinPrice || price > MaxPrice,
                new OrderItemPriceOutOfRangeError())
            .Build();
        
        if(result.IsFailure)
            return result;
        return new OrderItemPrice(price);
    }

    public sealed record OrderItemPriceOutOfRangeError(): Error("Price is out of range", $"Price must be between {MinPrice} and {MaxPrice}");
}
