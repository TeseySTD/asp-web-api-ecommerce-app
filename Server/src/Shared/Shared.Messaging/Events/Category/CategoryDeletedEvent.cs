namespace Shared.Messaging.Events.Category;

public record CategoryDeletedEvent(Guid CategoryId) : IntegrationEvent;