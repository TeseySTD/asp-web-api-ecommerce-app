using Basket.API.Application.UseCases.Cart.Commands.SaveCart;
using Basket.API.Application.UseCases.Cart.Queries.GetCartByUserId;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Http.Cart.Requests;
using Basket.API.Http.Cart.Responses;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        app.MapGet("/{id:guid}", async (
            ISender sender,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCartByUserIdQuery(id);

            var result = await sender.Send(query, cancellationToken);

            return result.Map(
                onSuccess: value => Results.Ok(value.Adapt<ProductCartDto>().Adapt<GetCartResponse>()),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapPost("/", async (
            ISender sender,
            SaveCartRequest request,
            CancellationToken cancellationToken) =>
        {
            var cmd = new SaveCartCommand(request.Dto);

            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });
    }
}