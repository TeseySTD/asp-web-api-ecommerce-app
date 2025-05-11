using Basket.API.Models.Cart.Entities;

namespace Basket.API.Dto.Cart;

public record ProductCartDto(Guid UserId, IEnumerable<ProductCartItemDto> Items);