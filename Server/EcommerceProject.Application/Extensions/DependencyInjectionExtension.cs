using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace EcommerceProject.Application.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services){
        services.AddMediatR( cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
}
