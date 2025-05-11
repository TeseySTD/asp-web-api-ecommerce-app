using Basket.API.Data.Abstractions;
using Basket.API.Data.Repository;
using Basket.API.Models.Cart;
using Marten;
using Weasel.Core;

namespace Basket.API.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(cfg =>
            {
                cfg.Connection(configuration.GetConnectionString("Database") 
                               ?? throw new InvalidOperationException("Database connection string is not set."));
                
                cfg.Schema.For<ProductCart>().Identity(x => x.Id);
                
                cfg.UseSystemTextJsonForSerialization();
            })
            .UseLightweightSessions();
        
        services.AddScoped<ICartRepository, CartRepository>();
        return services;
    }
}