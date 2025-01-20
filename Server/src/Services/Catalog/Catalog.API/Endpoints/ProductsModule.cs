using Carter;
using Catalog.API.Http.Product.Requests;
using Catalog.API.Http.Product.Responses;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.CreateProduct;
using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Application.UseCases.Products.Commands.UpdateProduct;
using Catalog.Application.UseCases.Products.Queries.GetProductById;
using Catalog.Application.UseCases.Products.Queries.GetProducts;
using Catalog.Core.Models.Products.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.API;
using Shared.Core.Auth;

namespace Catalog.API.Endpoints;

public class ProductModule : CarterModule
{
    public ProductModule() : base("/api/products")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.Map(
                onSuccess: value => Results.Ok(new GetProductsResponse(value)),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapGet("/{id:guid}", async (ISender sender, Guid id, CancellationToken cancellationToken) =>
        {
            var query = new GetProductByIdQuery(Id: ProductId.Create(id).Value);
            var result = await sender.Send(query, cancellationToken);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapPost("/", async (ISender sender, AddProductRequest request, CancellationToken cancellationToken) =>
        {
            var writeDto = new ProductWriteDto(
                Id: Guid.NewGuid(),
                Title: request.Title,
                Description: request.Description,
                Price: request.Price,
                Quantity: request.Quantity,
                CategoryId: request.CategoryId
            );

            var cmd = new CreateProductCommand(writeDto);
            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(writeDto.Id),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        {
            var updateDto = new ProductUpdateDto(
                Id: id,
                Title: request.Title,
                Description: request.Description,
                Price: request.Price,
                Quantity: request.Quantity,
                CategoryId: request.CategoryId
            );

            var cmd = new UpdateProductCommand(updateDto);
            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });

        app.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
        {
            var cmd = new DeleteProductCommand(ProductId.Create(id).Value);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });
    }
}