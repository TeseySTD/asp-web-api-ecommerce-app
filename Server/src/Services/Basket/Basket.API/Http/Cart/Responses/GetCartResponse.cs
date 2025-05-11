namespace Basket.API.Http.Cart.Responses;

public record GetCartResponse(Guid UserId, IEnumerable<ProductCartItemResponse> Items);