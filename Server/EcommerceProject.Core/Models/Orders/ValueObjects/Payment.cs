namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record Payment
{
    public const int MaxCardNameLength = 50;
    public const int MaxCardNumberLength = 50;
    public const int MaxExpirationLength = 50;
    public const int CVVLength = 3;
    
    
    public string? CardName { get; init; } = default!;
    public string? CardNumber { get; init; } = default!;
    public string? Expiration { get; init; } = default!;
    public string? CVV { get; init; } = default!;
    public string? PaymentMethod { get; init; } = default!;

    protected Payment()
    {
    }

    private Payment(string? cardName, string? cardNumber, string? expiration, string? cvv, string? paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        CVV = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, string paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(cvv);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cvv.Length, CVVLength);

        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}