namespace Shared.Messaging.Events.Product;

public record ProductDeletedEvent(Guid ProductId) : IntegrationEvent;