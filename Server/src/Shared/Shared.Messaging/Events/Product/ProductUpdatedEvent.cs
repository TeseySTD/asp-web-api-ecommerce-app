namespace Shared.Messaging.Events.Product;

public record ProductUpdatedEvent(Guid ProductId, string Title, string Description) : IntegrationEvent;