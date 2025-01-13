using Carter;
using Catalog.API.Http.Category.Requests;
using Catalog.API.Http.Category.Responses;
using Catalog.Application.Dto.Category;
using Catalog.Application.UseCases.Categories.Commands.CreateCategory;
using Catalog.Application.UseCases.Categories.Commands.DeleteCategory;
using Catalog.Application.UseCases.Categories.Commands.UpdateCategory;
using Catalog.Application.UseCases.Categories.Queries.GetCategories;
using Catalog.Application.UseCases.Categories.Queries.GetCategoryById;
using Catalog.Core.Models.Categories.ValueObjects;
using MediatR;
using Shared.Core.API;

namespace Catalog.API.Endpoints;

public class CategoryModule : CarterModule
{
    public CategoryModule() : base("/api/categories")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender) =>
        {
            var query = new GetCategoriesQuery();
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

        app.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken) =>
        {
            var categoryDto = new CategoryDto(
                Id: id,
                Name: request.Name,
                Description: request.Description
            );

            var cmd = new UpdateCategoryCommand(categoryDto);
            var result = await sender.Send(cmd, cancellationToken);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
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
}