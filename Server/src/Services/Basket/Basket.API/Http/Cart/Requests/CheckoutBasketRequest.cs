namespace Basket.API.Http.Cart.Requests;

public record CheckoutBasketRequest(
    Guid UserId,
    string CardName, string CardNumber, string? Expiration, string CVV, string? PaymentMethod, // Payment
    string AddressLine, string? Country, string? State, string? ZipCode // Address
);