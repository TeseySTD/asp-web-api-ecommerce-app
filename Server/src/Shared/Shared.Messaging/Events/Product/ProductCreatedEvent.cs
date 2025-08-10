namespace Shared.Messaging.Events.Product;

public record ProductCreatedEvent(
    Guid ProductId,
    string Title,
    string Description) : IntegrationEvent;
    