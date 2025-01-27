namespace Shared.Messaging.Events.Order;

public record ReservedProductsEvent(Guid OrderId, IEnumerable<OrderItemApprovedDto> OrderItemsDtos) : IntegrationEvent;
