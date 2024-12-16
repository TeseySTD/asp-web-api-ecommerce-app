using System.Text.RegularExpressions;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Orders.ValueObjects;

public record Payment
{
    public const int MaxCardNameLength = 50;
    public const int MaxCardNumberLength = 19;
    public const string VisaMasterCardNumberRegex = "^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14})$";
    public const int MaxExpirationLength = 50;
    public const int CVVLength = 3;
    
    
    public string CardName { get; init; } = default!;
    public string CardNumber { get; init; } = default!;
    public string Expiration { get; init; } = default!;
    public string CVV { get; init; } = default!;
    public string PaymentMethod { get; init; } = default!;

    protected Payment()
    {
    }

    private Payment(string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration ?? "Unknown";
        CVV = cvv;
        PaymentMethod = paymentMethod ?? "Unknown";
    }

    public static Result<Payment> Create(string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod)
    {
        var result =  Result<Payment>.TryFail(new Payment(cardName, cardNumber, expiration, cvv, paymentMethod))
            .CheckError(string.IsNullOrWhiteSpace(cardName),
                new Error("Card name is required", "Card name is required"))
            .CheckError(string.IsNullOrWhiteSpace(cardNumber),
                new Error("Card number is required", "Card number is required"))
            .CheckError(!Regex.IsMatch(cardNumber, VisaMasterCardNumberRegex),
                new Error("Card number is invalid", "Card number must be a valid Visa Master card number."))
            .CheckError(string.IsNullOrWhiteSpace(cvv),
                new Error("CVV is required", "CVV is required"))
            .CheckErrorIf(
                !string.IsNullOrWhiteSpace(cvv),
                cvv.Length != CVVLength,
                new Error("CVV is out of range", $"CVV must be of length {CVVLength}"))
            .Build();
        
        return result;
    }
}