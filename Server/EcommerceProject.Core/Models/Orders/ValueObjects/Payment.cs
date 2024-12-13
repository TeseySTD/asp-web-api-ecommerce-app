using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record Payment
{
    public const int MaxCardNameLength = 50;
    public const int MaxCardNumberLength = 50;
    public const int MaxExpirationLength = 50;
    public const int CVVLength = 3;
    
    
    public string CardName { get; init; } = default!;
    public string CardNumber { get; init; } = default!;
    public string? Expiration { get; init; } = default!;
    public string CVV { get; init; } = default!;
    public string? PaymentMethod { get; init; } = default!;

    protected Payment()
    {
    }

    private Payment(string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        CVV = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Result<Payment> Create(string cardName, string cardNumber, string expiration, string cvv, string paymentMethod)
    {
        return Result<Payment>.TryFail(new Payment(cardName, cardNumber, expiration, cvv, paymentMethod))
            .CheckError(string.IsNullOrWhiteSpace(cardName),
                new Error("Card name is required", "Card name is required"))
            .CheckError(string.IsNullOrWhiteSpace(cardNumber),
                new Error("Card number is required", "Card number is required"))
            .CheckError(string.IsNullOrWhiteSpace(cvv),
                new Error("CVV is required", "CVV is required"))
            .DropIfFailed()
            .CheckError(cvv.Length != CVVLength,
                new Error("CVV is out of range", $"CVV must be of length {CVVLength}"))
            .Build();
    }
}