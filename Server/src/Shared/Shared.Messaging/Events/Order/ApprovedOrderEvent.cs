namespace Shared.Messaging.Events.Order;

public record ApprovedOrderEvent(Guid OrderId) : IntegrationEvent;