using System;
using EcommerceProject.Application.Services;
using EcommerceProject.Core.Abstractions.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EcommerceProject.Application.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services){
        services.AddTransient<IProductService, ProductService>();
        return services;
    }
}
