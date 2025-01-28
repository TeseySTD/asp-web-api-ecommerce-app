using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Images.ValueObjects;

namespace Catalog.API.Helpers;

public class ImageUrlGenerator : IImageUrlGenerator
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ImageUrlGenerator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GenerateUrl(ImageId imageId)
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        return $"{request.Scheme}://localhost:{request.Host.Port}/api/images/{imageId.Value}";
    }
}