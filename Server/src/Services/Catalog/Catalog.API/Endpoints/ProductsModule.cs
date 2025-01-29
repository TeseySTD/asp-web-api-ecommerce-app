using Carter;
using Catalog.API.Http.Product.Requests;
using Catalog.API.Http.Product.Responses;
using Catalog.Application.Dto.Image;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.AddProductImages;
using Catalog.Application.UseCases.Products.Commands.CreateProduct;
using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Application.UseCases.Products.Commands.UpdateProduct;
using Catalog.Application.UseCases.Products.Queries.GetProductById;
using Catalog.Application.UseCases.Products.Queries.GetProducts;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.API;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;

namespace Catalog.API.Endpoints;

public class ProductModule : CarterModule
{
    public ProductModule() : base("/api/products")
    {
        WithTags("Products");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender,
            [AsParameters]PaginationRequest paginationRequest,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(paginationRequest);
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

        // Create product
        app.MapPost("/", async (ISender sender, AddProductRequest request,
            CancellationToken cancellationToken) =>
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

        // Add images to product
        app.MapPost("/{id:guid}/images", async (ISender sender,
                Guid id,
                [FromForm] IFormFileCollection images, CancellationToken cancellationToken) =>
            {
                var imageDtos = new List<ImageDto>();
                var validationResult = Result.Try()
                    .Check(images == null, ProductsModuleErrors.RequiredImageError)
                    .Check(images!.Count() == 0 || images.Count() > Product.MaxImagesCount, ProductsModuleErrors.ImageCountOutOfRangeError)
                    .Check(() =>
                    {
                        foreach (var image in images)
                            if (!Enum.GetNames<ImageContentType>().Select(t => $"image/{t.ToLower()}")
                                    .Contains(image.ContentType))
                                return true;

                        return false;
                    }, ProductsModuleErrors.FilesMustBeImages)
                    .Build();
                if (validationResult.IsFailure)
                    return Results.BadRequest(Envelope.Of(validationResult.Errors));

                foreach (var image in images)
                {
                    using var ms = new MemoryStream();
                    await image.CopyToAsync(ms, cancellationToken);

                    imageDtos.Add(new ImageDto(
                        FileName: image.FileName,
                        ContentType: image.ContentType.Replace("image/", ""),
                        Data: ms.ToArray()
                    ));
                }

                var cmd = new AddProductImagesCommand(id, imageDtos);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(id),
                    onFailure: errors => Results.BadRequest(Envelope.Of(errors))
                );
            })
            .DisableAntiforgery()
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapPut("/{id:guid}",
            async (ISender sender, Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
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

public static class ProductsModuleErrors
{
    public static Error RequiredImageError => new("Images are required", "Image files are required");

    public static Error ImageCountOutOfRangeError =>
        new("Images count is out of range", $"Images count must be between 1 and {Product.MaxImagesCount}");

    public static Error FilesMustBeImages => new("All files must be images",
        $"All files must be images with content type:{string.Join(" ,", Enum.GetNames<ImageContentType>())}");
}