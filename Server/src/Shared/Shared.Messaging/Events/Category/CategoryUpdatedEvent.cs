namespace Shared.Messaging.Events.Category;

public record CategoryUpdatedEvent(
    Guid CategoryId,
    string CategoryName) : IntegrationEvent;