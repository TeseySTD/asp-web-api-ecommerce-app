namespace Shared.Messaging.Events.Product;

public record ProductUpdatedEvent(
    Guid ProductId,
    string Title,
    string Description,
    decimal Price,
    ProductUpdatedEventCategory? Category,
    string[] ImageUrls) : IntegrationEvent;
    
public record ProductUpdatedEventCategory(Guid CategoryId, string Title);