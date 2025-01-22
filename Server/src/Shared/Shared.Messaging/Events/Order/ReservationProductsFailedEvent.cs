namespace Shared.Messaging.Events.Order;

public record ReservationProductsFailedEvent(Guid OrderId, string Reason) : IntegrationEvent;