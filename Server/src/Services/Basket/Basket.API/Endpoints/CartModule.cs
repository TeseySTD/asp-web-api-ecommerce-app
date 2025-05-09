using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Carter;
using Shared.Core.API;
using Shared.Core.Validation.Result;

namespace Basket.API.Endpoints;

public class CartModule : CarterModule
{
    public CartModule() : base("/api/cart")
    {
        WithTags("Cart");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", async (ICartRepository repository, Guid id) =>
        {
            var result = await repository.GetCart(UserId.Create(id).Value);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapPost("/", async (ICartRepository repository) =>
        {
            var cart = ProductCart.Create(UserId.From(Guid.NewGuid()));

            var result = await repository.StoreCart(cart);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });
    }
}
