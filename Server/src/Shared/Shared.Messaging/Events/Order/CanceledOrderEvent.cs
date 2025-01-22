namespace Shared.Messaging.Events.Order;

public record CanceledOrderEvent(Guid OrderId, string Reason) : IntegrationEvent;