namespace Shared.Messaging.Events.Order;

public record ApprovedOrderEvent(Guid OrderId, IEnumerable<OrderItemApprovedDto> OrderItemsDtos) : IntegrationEvent;

public record OrderItemApprovedDto(Guid Id, string ProductTitle, string ProductDescription, uint Quantity, decimal UnitPrice);