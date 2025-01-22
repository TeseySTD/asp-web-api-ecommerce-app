namespace Shared.Messaging.Events.Order;

public record CheckedCustomerEvent(Guid OrderId) : IntegrationEvent;