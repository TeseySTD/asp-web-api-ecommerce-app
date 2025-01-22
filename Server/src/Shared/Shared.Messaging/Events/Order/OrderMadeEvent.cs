namespace Shared.Messaging.Events.Order;

public record OrderMadeEvent(Guid OrderId, Guid CustomerId, List<ProductWithQuantityDto> Products) : IntegrationEvent;


public record ProductWithQuantityDto(Guid ProductId, uint ProductQuantity);