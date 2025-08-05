using System.Security.Claims;
using Carter;
using Catalog.API.Http.Product.Requests;
using Catalog.API.Http.Product.Responses;
using Catalog.Application.Dto.Image;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.AddProductImages;
using Catalog.Application.UseCases.Products.Commands.CreateProduct;
using Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;
using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Application.UseCases.Products.Commands.DeleteProductImage;
using Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;
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
            [AsParameters] PaginationRequest paginationRequest,
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
        app.MapPost("/", async (ISender sender, AddProductRequest request, ClaimsPrincipal userClaims,
            CancellationToken cancellationToken) =>
        {
            if (ExtractUserDataFromClaims(userClaims).IsFailure)
                return Results.Unauthorized();
            var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

            var writeDto = new ProductWriteDto(
                Id: Guid.NewGuid(),
                Title: request.Title,
                Description: request.Description,
                Price: request.Price,
                Quantity: request.Quantity,
                SellerId: userId,
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
                [FromForm] IFormFileCollection images, ClaimsPrincipal userClaims,
                CancellationToken cancellationToken) =>
            {
                if (ExtractUserDataFromClaims(userClaims).IsFailure)
                    return Results.Unauthorized();
                var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

                var imageDtos = new List<ImageDto>();
                var validationResult = Result.Try()
                    .Check(images == null, new RequiredImageError())
                    .DropIfFail()
                    .Check(images!.Count() == 0 || images!.Count() > Product.MaxImagesCount,
                        new ImageCountOutOfRangeError())
                    .Check(() =>
                    {
                        foreach (var image in images!)
                            if (!Enum.GetNames<ImageContentType>().Select(t => $"image/{t.ToLower()}")
                                    .Contains(image.ContentType))
                                return true;

                        return false;
                    }, new InvalidImagesTypeError())
                    .Build();
                if (validationResult.IsFailure)
                    return Results.BadRequest(Envelope.Of(validationResult.Errors));

                foreach (var image in images!)
                {
                    using var ms = new MemoryStream();
                    await image.CopyToAsync(ms, cancellationToken);

                    imageDtos.Add(new ImageDto(
                        FileName: image.FileName,
                        ContentType: image.ContentType.Replace("image/", ""),
                        Data: ms.ToArray()
                    ));
                }

                var cmd = new AddProductImagesCommand(id, userId, imageDtos);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(id),
                    onFailure: errors =>
                    {
                        var enumerable = errors as Error[] ?? errors.ToArray();
                        if (enumerable.Any(e => e is AddProductImagesCommandHandler.ProductNotFoundError))
                            return Results.NotFound(Envelope.Of(enumerable));
                        else if (enumerable.Any(e => e is AddProductImagesCommandHandler.CustomerMismatchError))
                            return Results.Forbid();
                        return Results.BadRequest(Envelope.Of(enumerable));
                    }
                );
            })
            .DisableAntiforgery()
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapDelete("/{id:guid}/images/{imageId:guid}", async (ISender sender,
                Guid id, Guid imageId, ClaimsPrincipal userClaims, CancellationToken cancellationToken) =>
            {
                if (ExtractUserDataFromClaims(userClaims).IsFailure)
                    return Results.Unauthorized();
                var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

                var cmd = new DeleteProductImageCommand(id, userId, imageId);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(),
                    onFailure: errors =>
                    {
                        var enumerable = errors as Error[] ?? errors.ToArray();

                        if (enumerable.Any(e =>
                                e is DeleteProductImageCommandHandler.ProductNotFoundError ||
                                e is DeleteProductImageCommandHandler.ImageNotFoundError))
                            return Results.NotFound(Envelope.Of(enumerable));
                        else if (enumerable.Any(e => e is DeleteProductImageCommandHandler.CustomerMismatchError))
                            return Results.Forbid();
                        return Results.BadRequest(Envelope.Of(enumerable));
                    }
                );
            })
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapPut("/{id:guid}",
                async (ISender sender, Guid id, UpdateProductRequest request, ClaimsPrincipal userClaims,
                    CancellationToken cancellationToken) =>
                {
                    if (ExtractUserDataFromClaims(userClaims).IsFailure)
                        return Results.Unauthorized();
                    var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

                    var updateDto = new ProductUpdateDto(
                        Id: id,
                        Title: request.Title,
                        Description: request.Description,
                        Price: request.Price,
                        Quantity: request.Quantity,
                        SellerId: request.SellerId,
                        CategoryId: request.CategoryId
                    );

                    var cmd = new UpdateProductCommand(userId, updateDto);
                    var result = await sender.Send(cmd, cancellationToken);

                    return result.Map(
                        onSuccess: () => Results.Ok(),
                        onFailure: errors =>
                        {
                            var enumerable = errors as Error[] ?? errors.ToArray();
                            if(enumerable.Any(e => e is UpdateProductCommandHandler.CustomerMismatchError))
                                return Results.Forbid();
                            return Results.NotFound(Envelope.Of(enumerable));
                        }
                    );
                })
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapPut("/increase-quantity/{id:guid}/{quantity:int}",
                async (ISender sender, Guid id, int quantity, ClaimsPrincipal userClaims,
                    CancellationToken cancellationToken) =>
                {
                    if (ExtractUserDataFromClaims(userClaims).IsFailure)
                        return Results.Unauthorized();
                    var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

                    var cmd = new IncreaseQuantityCommand(id, userId, quantity);
                    var result = await sender.Send(cmd, cancellationToken);

                    return result.Map(
                        onSuccess: () => Results.Ok(),
                        onFailure: errors =>
                        {
                            var enumerable = errors as Error[] ?? errors.ToArray();
                            if(enumerable.Any(e => e is IncreaseQuantityCommandHandler.ProductNotFoundError))
                                return Results.NotFound(Envelope.Of(enumerable));
                            else if (enumerable.Any(e => e is IncreaseQuantityCommandHandler.CustomerMismatchError))
                                return Results.Forbid();
                            return Results.BadRequest(Envelope.Of(enumerable));
                        }
                    );
                })
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapPut("/decrease-quantity/{id:guid}/{quantity:int}",
                async (ISender sender, Guid id, int quantity, ClaimsPrincipal userClaims,
                    CancellationToken cancellationToken) =>
                {
                    if (ExtractUserDataFromClaims(userClaims).IsFailure)
                        return Results.Unauthorized();
                    var userId = ExtractUserDataFromClaims(userClaims).Value.userId;

                    var cmd = new DecreaseQuantityCommand(id, userId, quantity);
                    var result = await sender.Send(cmd, cancellationToken);

                    return result.Map(
                        onSuccess: () => Results.Ok(),
                        onFailure: errors =>
                        {
                            var enumerable = errors as Error[] ?? errors.ToArray();
                            if (enumerable.Any(e => e is DecreaseQuantityCommandHandler.ProductNotFoundError))
                                return Results.NotFound(Envelope.Of(enumerable));
                            else if (enumerable.Any(e => e is DecreaseQuantityCommandHandler.CustomerMismatchError))
                                return Results.Forbid();
                            return Results.BadRequest(Envelope.Of(enumerable));
                        }
                    );
                })
            .RequireAuthorization(Policies.RequireSellerPolicy);

        app.MapDelete("/{id:guid}", async (ISender sender, Guid id, ClaimsPrincipal userClaims) =>
            {
                if (ExtractUserDataFromClaims(userClaims).IsFailure)
                    return Results.Unauthorized();
                var (userId, role) = ExtractUserDataFromClaims(userClaims).Value;

                var cmd = new DeleteProductCommand(ProductId.Create(id).Value, SellerId.Create(userId).Value, role);
                var result = await sender.Send(cmd);

                return result.Map(
                    onSuccess: () => Results.Ok(),
                    onFailure: errors =>
                    {
                        var enumerable = errors as Error[] ?? errors.ToArray();
                        if (enumerable.Any(e => e is DeleteProductCommandHandler.ProductNotFoundError))
                            return Results.NotFound(Envelope.Of(enumerable));
                        else if (enumerable.Any(e => e is DeleteProductCommandHandler.CustomerMismatchError))
                            return Results.Forbid();
                        return Results.BadRequest(Envelope.Of(enumerable));
                    }
                );
            })
            .RequireAuthorization(Policies.RequireSellerPolicy);
    }

    public sealed record RequiredImageError() : Error("Images are required", "Image files are required");

    public sealed record ImageCountOutOfRangeError() : Error("Images count is out of range",
        $"Images count must be between 1 and {Product.MaxImagesCount}");

    public sealed record InvalidImagesTypeError() : Error("All files must be images",
        $"All files must be images with content type:{string.Join(" ,", Enum.GetNames<ImageContentType>())}");


    protected Result<(Guid userId, UserRole userRole)> ExtractUserDataFromClaims(ClaimsPrincipal userClaims)
    {
        var userIdString = userClaims.FindFirstValue("userId");
        var customerRoleString = userClaims.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(customerRoleString))
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid claims", "Missing required user claims"));
        }

        if (!Guid.TryParse(userIdString, out Guid userId) || userId == Guid.Empty)
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid user ID",
                "User ID claim is not a valid GUID"));
        }

        if (!Enum.TryParse(customerRoleString, out UserRole userRole))
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid role", "User role claim is not valid"));
        }

        return Result<(Guid, UserRole)>.Success((userId, userRole));
    }
}