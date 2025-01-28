using Catalog.Core.Models.Images.ValueObjects;

namespace Catalog.Application.Common.Interfaces;

public interface IImageUrlGenerator
{
    string GenerateUrl(ImageId imageId);
}