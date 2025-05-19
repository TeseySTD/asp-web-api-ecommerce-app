using Carter;
using Catalog.Application.UseCases.Images.Queries.GetImageById;
using Catalog.Core.Models.Images;
using MediatR;
using Shared.Core.API;

namespace Catalog.API.Endpoints;

public class ImagesModule : CarterModule
{
    public ImagesModule() : base("/api/images")
    {
        WithTags("Images");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
        {
            var query = new GetImageByIdQuery(id);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: image =>
                {
                    var contentType = image.ContentType.ToString().ToLower();
                    if (image.ContentType == ImageContentType.SVG)
                    {
                        contentType = "image/svg+xml";
                    }
                    else
                    {
                        contentType = $"image/{contentType}";
                    }

                    return Results.File(
                        image.Data.Value,
                        contentType: contentType,
                        enableRangeProcessing: true
                    );
                },
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}