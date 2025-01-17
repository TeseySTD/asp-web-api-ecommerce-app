namespace Shared.Messaging.Events.Order;

public record CancelOrderEvent(Guid OrderId, string Reason) : IntegrationEvent;