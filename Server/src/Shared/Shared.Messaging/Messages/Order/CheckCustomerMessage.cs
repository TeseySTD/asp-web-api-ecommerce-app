namespace Shared.Messaging.Messages.Order;

public record CheckCustomerMessage(Guid OrderId, Guid CustomerId) : IntegrationMessage;