namespace Ordering.Application.Dto.Order;

public record OrderWriteDto(
    Guid UserId,
    IEnumerable<(Guid ProductId, string ProductName, string ProductDescription, uint Quantity, uint Price)> OrderItems,
    (string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod) Payment,
    (string addressLine, string? country, string? state, string? zipCode) DestinationAddress
);