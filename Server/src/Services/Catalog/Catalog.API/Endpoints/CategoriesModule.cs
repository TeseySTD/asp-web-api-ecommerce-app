using Carter;
using Catalog.API.Http.Category.Requests;
using Catalog.API.Http.Category.Responses;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Image;
using Catalog.Application.UseCases.Categories.Commands.AddCategoryImages;
using Catalog.Application.UseCases.Categories.Commands.CreateCategory;
using Catalog.Application.UseCases.Categories.Commands.DeleteCategory;
using Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;
using Catalog.Application.UseCases.Categories.Commands.UpdateCategory;
using Catalog.Application.UseCases.Categories.Queries.GetCategories;
using Catalog.Application.UseCases.Categories.Queries.GetCategoryById;
using Catalog.Application.UseCases.Products.Commands.AddProductImages;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.API;
using Shared.Core.Validation.Result;

namespace Catalog.API.Endpoints;

public class CategoryModule : CarterModule
{
    public CategoryModule() : base("/api/categories")
    {
        WithTags("Categories");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender,
            [AsParameters] PaginationRequest request) =>
        {
            var query = new GetCategoriesQuery(request);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(new GetCategoriesResponse(value)),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
        {
            var query = new GetCategoryByIdQuery(CategoryId.Create(id).Value);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        app.MapPost("/", async (ISender sender, AddCategoryRequest request, CancellationToken cancellationToken) =>
        {
            var cmd = new CreateCategoryCommand(
                request.Name,
                request.Description
            );
            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: value => Results.Ok(value.Value),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });

        app.MapPost("/{id:guid}/images", async (ISender sender,
                Guid id,
                [FromForm] IFormFileCollection images, CancellationToken cancellationToken) =>
            {
                var imageDtos = new List<ImageDto>();
                var validationResult = Result.Try()
                    .Check(images == null, new RequiredImageError())
                    .DropIfFail()
                    .Check(images!.Count() == 0 || images!.Count() > Category.MaxImagesCount, new ImageCountOutOfRangeError())
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

                var cmd = new AddCategoryImagesCommand(id, imageDtos);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(id),
                    onFailure: errors =>
                    {
                        var enumerable = errors as Error[] ?? errors.ToArray();
                        if (enumerable.Any(e => e is AddCategoryImagesCommandHandler.CategoryNotFoundError))
                            return Results.NotFound(Envelope.Of(enumerable));
                        return Results.BadRequest(Envelope.Of(enumerable));
                    } 
                );
            })
            .DisableAntiforgery();

        app.MapDelete("/{id:guid}/images/{imageId:guid}",
            async (ISender sender, Guid id, Guid imageId, CancellationToken cancellationToken) =>
            {
                var cmd = new DeleteCategoryImageCommand(id, imageId);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(),
                    onFailure: errors => Results.NotFound(Envelope.Of(errors))
                );
            });

        app.MapPut("/{id:guid}",
            async (ISender sender, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken) =>
            {
                var categoryDto = new CategoryWriteDto(
                    Id: id,
                    Name: request.Name,
                    Description: request.Description
                );

                var cmd = new UpdateCategoryCommand(categoryDto);
                var result = await sender.Send(cmd, cancellationToken);

                return result.Map(
                    onSuccess: () => Results.Ok(),
                    onFailure: errors =>
                    {
                        var enumerable = errors as Error[] ?? errors.ToArray();
                        if(enumerable.Any(e => e == Error.NotFound))
                            return Results.NotFound(Envelope.Of(enumerable));
                        return Results.BadRequest(Envelope.Of(enumerable));
                    }
                );
            });

        app.MapDelete("/{id:guid}", async (ISender sender, Guid id, CancellationToken cancellationToken) =>
        {
            var cmd = new DeleteCategoryCommand(CategoryId.Create(id).Value);
            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });
    }

    public sealed record RequiredImageError() : Error("Images are required", "Image files are required");

    public sealed record ImageCountOutOfRangeError() : Error("Images count is out of range",
        $"Images count must be between 1 and {Category.MaxImagesCount}");

    public sealed record InvalidImagesTypeError() : Error("All files must be images",
        $"All files must be images with content type:{string.Join(" ,", Enum.GetNames<ImageContentType>())}");
}