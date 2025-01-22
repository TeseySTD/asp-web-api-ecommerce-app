using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Interfaces;
using Ordering.Persistence.Interceptors;

namespace Ordering.Persistence;

public static class DependecyInjection
{
    public static IServiceCollection AddPersistenceLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Add interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        //Add db context
        services.AddDbContext<IApplicationDbContext,ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });
        
        return services;
    }
}