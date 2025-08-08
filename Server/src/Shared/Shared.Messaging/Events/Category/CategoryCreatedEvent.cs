namespace Shared.Messaging.Events.Category;

public record CategoryCreatedEvent(
    Guid CategoryId,
    string CategoryName) : IntegrationEvent;