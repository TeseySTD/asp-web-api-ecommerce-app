namespace Ordering.Application.Dto.Order;

public record OrderUpdateDto(
    (string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod) Payment,
    (string addressLine, string? country, string? state, string? zipCode) DestinationAddress
);