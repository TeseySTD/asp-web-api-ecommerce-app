namespace Ordering.API.Http.Order.Requests;

public record CreateOrderItemRequest(Guid ProductId, string ProductName, string ProductDescription, uint Quantity, uint Price);