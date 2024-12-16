namespace EcommerceProject.API.Http.Order.Requests;

public record MakeOrderRequest(
    Guid UserId,
    CreateOrderItemRequest[] OrderItems,
    string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod, // Payment
    string addressLine, string? country, string? state, string? zipCode // Address
);

