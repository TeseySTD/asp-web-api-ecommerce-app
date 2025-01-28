using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Images.ValueObjects;

public record ImageData
{
    public byte[] Value { get; init; }
    public const int MaxSize = 16 * 1024 * 1024;

    private ImageData(byte[] value)
    {
        Value = value;
    }

    public static Result<ImageData> Create(byte[] value)
    {
        return Result<ImageData>.Try(new ImageData(value))
            .Check(value == null || value.Length == 0, new("Data is required", "Data cannot be null."))
            .DropIfFail()
            .Check(value!.Length > MaxSize,
                new("Image size is out of range", $"Image size must be between 0 and {MaxSize / (1024 * 1024)}mb"))
            .Build();
    }
}