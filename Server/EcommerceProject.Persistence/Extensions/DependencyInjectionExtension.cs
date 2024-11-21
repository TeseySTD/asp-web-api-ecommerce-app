using System;
using EcommerceProject.Core.Abstractions.Interfaces.Repositories;
using EcommerceProject.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceProject.Persistence.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddPersistenceLayerServices(this IServiceCollection services, IConfiguration configuration){
        //Add db context
        services.AddDbContext<StoreDbContext>(
            options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(StoreDbContext)));
            }
        );

        //Add repositories
        services.AddScoped<IProductsRepository, ProductsRepository>();

        return services;
    }
}
