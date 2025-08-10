namespace Basket.API.Dto.Cart;

public record ProductCartDto(Guid UserId, IEnumerable<ProductCartItemDto> Items);