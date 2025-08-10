using System.Text.RegularExpressions;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Orders.ValueObjects;

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
        return Result<Payment>.Try(new Payment(cardName, cardNumber, expiration, cvv, paymentMethod))
            .Check(string.IsNullOrWhiteSpace(cardName), new CardNameRequiredError())
            .Check(string.IsNullOrWhiteSpace(cardNumber), new CardNumberRequiredError())
            .Check(!Regex.IsMatch(cardNumber, VisaMasterCardNumberRegex), new InvalidCardNumberError(cardNumber))
            .Check(string.IsNullOrWhiteSpace(cvv), new CVVRequiredError())
            .CheckIf(
                !string.IsNullOrWhiteSpace(cvv),
                cvv.Length != CVVLength,
                new CVVLengthError(cvv.Length))
            .Build();
    }

    public sealed record CardNameRequiredError()
        : Error("Card name is required", "Card name is required");

    public sealed record CardNumberRequiredError()
        : Error("Card number is required", "Card number is required");

    public sealed record InvalidCardNumberError(string CardNumber)
        : Error("Card number is invalid", $"Card number '{CardNumber}' must be a valid Visa or MasterCard number.");

    public sealed record CVVRequiredError()
        : Error("CVV is required", "CVV is required");

    public sealed record CVVLengthError(int Length)
        : Error("CVV is out of range", $"CVV must be exactly {CVVLength} digits, but got {Length}.");

}