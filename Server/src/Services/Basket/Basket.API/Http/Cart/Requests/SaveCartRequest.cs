using Basket.API.Dto.Cart;
using Basket.API.Models.Cart.Entities;

namespace Basket.API.Http.Cart.Requests;

public record SaveCartRequest(ProductCartDto Dto);