namespace Shared.Messaging.Events.Order;

public record ReservedProductsEvent(Guid OrderId) : IntegrationEvent;
