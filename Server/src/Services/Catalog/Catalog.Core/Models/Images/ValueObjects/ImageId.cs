using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Images.ValueObjects;

public record ImageId
{
    public Guid Value { get; init; }

    private ImageId(Guid value)
    {
        Value = value;
    }
    
    public static Result<ImageId> Create(Guid imageId)
    {
        return Result<ImageId>.Try(new ImageId(imageId))
            .Check(imageId == Guid.Empty, new ImageIdRequiredError())
            .Build();
    }

    public sealed record ImageIdRequiredError() : Error("ImageId is required", "ImageId cannot be null or empty");
}