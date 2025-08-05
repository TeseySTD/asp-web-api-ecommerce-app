namespace Ordering.API.Http.Order.Requests;

public record MakeOrderRequest(
    CreateOrderItemRequest[] OrderItems,
    string CardName, string CardNumber, string? Expiration, string CVV, string? PaymentMethod, // Payment
    string AddressLine, string? Country, string? State, string? ZipCode // Address
);

