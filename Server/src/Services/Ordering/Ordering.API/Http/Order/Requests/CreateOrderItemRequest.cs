namespace Ordering.API.Http.Order.Requests;

public record CreateOrderItemRequest(Guid ProductId, uint Quantity);