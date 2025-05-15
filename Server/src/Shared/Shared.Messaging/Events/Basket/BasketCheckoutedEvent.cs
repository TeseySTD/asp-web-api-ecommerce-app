using Shared.Messaging.Events.Order;

namespace Shared.Messaging.Events.Basket;

public record BasketCheckoutedEvent(
    Guid UserId,
    IEnumerable<ProductWithQuantityDto> Products,
    BasketCheckoutedEventPayment Payment,
    BasketCheckoutedEventDestinationAddress DestinationAddress
) : IntegrationEvent;

public record BasketCheckoutedEventPayment(
    string CardName,
    string CardNumber,
    string? Expiration,
    string Cvv,
    string? PaymentMethod
);

public record BasketCheckoutedEventDestinationAddress(
    string AddressLine,
    string? Country,
    string? State,
    string? ZipCode
);