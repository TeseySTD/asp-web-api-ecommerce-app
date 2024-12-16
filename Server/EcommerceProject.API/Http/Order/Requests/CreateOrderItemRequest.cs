namespace EcommerceProject.API.Http.Order.Requests;

public record CreateOrderItemRequest(Guid ProductId, uint Quantity, uint Price);