namespace Ordering.API.Http.Order.Requests;

public record UpdateOrderRequest(
    string CardName, string CardNumber, string? Expiration, string CVV, string? PaymentMethod, // Payment
    string AddressLine, string? Country, string? State, string? ZipCode // Address
);