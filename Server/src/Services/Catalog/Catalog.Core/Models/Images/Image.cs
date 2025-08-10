using Catalog.Core.Models.Images.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Catalog.Core.Models.Images;

public class Image : AggregateRoot<ImageId>
{
    public ImageData Data { get; set; }
    public FileName FileName { get; set; }
    public ImageContentType ContentType { get; set; }

    private Image(ImageId id, FileName fileName, ImageData data, ImageContentType contentType)
    {
        Id = id;
        FileName = fileName;
        Data = data;
        ContentType = contentType;
    }

    public static Image Create(ImageId id, FileName fileName, ImageData data, ImageContentType contentType)
        => new(id, fileName, data, contentType);
    public static Image Create(FileName fileName, ImageData data, ImageContentType contentType)
        => new(ImageId.Create(Guid.NewGuid()).Value, fileName, data, contentType);
}

public enum ImageContentType
{
    JPEG = 1,
    PNG,
    SVG,
    GIF
}