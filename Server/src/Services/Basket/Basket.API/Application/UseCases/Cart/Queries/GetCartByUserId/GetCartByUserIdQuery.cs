using Basket.API.Models.Cart;
using Shared.Core.CQRS;

namespace Basket.API.Application.UseCases.Cart.Queries.GetCartByUserId;

public record GetCartByUserIdQuery(Guid UserId) : IQuery<ProductCart>;