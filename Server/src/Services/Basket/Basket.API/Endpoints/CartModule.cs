using Basket.API.Application.UseCases.Cart.Commands.DeleteCart;
using Basket.API.Application.UseCases.Cart.Commands.RemoveProduct;
using Basket.API.Application.UseCases.Cart.Commands.SaveCart;
using Basket.API.Application.UseCases.Cart.Commands.StoreProduct;
using Basket.API.Application.UseCases.Cart.Queries.GetCartByUserId;
using Basket.API.Dto.Cart;
using Basket.API.Http.Cart.Requests;
using Basket.API.Http.Cart.Responses;
using Carter;
using Mapster;
using MediatR;
using Shared.Core.API;

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
        }).WithName("Get Cart");

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
        }).WithName("Save Cart");
        
        app.MapDelete("/{userId:guid}", async (
            ISender sender,
            Guid userId,
            CancellationToken cancellationToken) =>
        {
            var cmd = new DeleteCartCommand(userId);

            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).WithName("Delete Cart");
        
        app.MapDelete("/{userId:guid}/{productId:guid}", async (
            ISender sender,
            Guid userId,
            Guid productId,
            CancellationToken cancellationToken) =>
        {
            var cmd = new RemoveProductCommand(userId, productId);

            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).WithName("Remove Product From Cart");

        app.MapPost("/{userId:guid}", async (
            ISender sender,
            Guid userId,
            ProductCartItemDto product,
            CancellationToken cancellationToken) =>
        {
            var cmd = new StoreProductCommand(userId, product);

            var result = await sender.Send(cmd, cancellationToken);
            
            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).WithName("Add Product To Cart");
    }
}