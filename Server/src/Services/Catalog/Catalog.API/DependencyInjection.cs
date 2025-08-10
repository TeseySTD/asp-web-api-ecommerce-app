using Catalog.API.Helpers;
using Catalog.Application.Common.Interfaces;

namespace Catalog.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiLayerServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IImageUrlGenerator, ImageUrlGenerator>();
        
        return services;
    }
}