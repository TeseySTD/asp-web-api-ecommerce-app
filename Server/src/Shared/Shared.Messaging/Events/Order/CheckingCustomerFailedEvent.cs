using Shared.Messaging.Messages;

namespace Shared.Messaging.Events.Order;

public record CheckingCustomerFailedEvent(Guid OrderId) : IntegrationEvent;