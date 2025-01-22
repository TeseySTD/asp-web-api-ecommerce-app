using Catalog.Application.Common.Interfaces;
using Catalog.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceLayerServices(this IServiceCollection services, IConfiguration configuration){

        //Add interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        //Add db context
        services.AddDbContext<IApplicationDbContext,ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });
        
        //Add distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Catalog";
        });
        
        return services;
    }
}
