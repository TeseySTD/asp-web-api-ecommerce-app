namespace Basket.API.Dto.Cart;

public record CheckoutBasketDto(
    Guid UserId,
    (string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod) Payment,
    (string addressLine, string? country, string? state, string? zipCode) DestinationAddress
);