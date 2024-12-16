namespace EcommerceProject.API.Http.Order.Requests;

public record UpdateOrderRequest(
    CreateOrderItemRequest[] OrderItems,
    string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod, //Payment
    string addressLine, string? country, string? state, string? zipCode // Address
);