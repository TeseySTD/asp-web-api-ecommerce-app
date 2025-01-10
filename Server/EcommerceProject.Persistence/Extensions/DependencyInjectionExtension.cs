using System;
using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Persistence.Interceptors;
using EcommerceProject.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceProject.Persistence.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddPersistenceLayerServices(this IServiceCollection services, IConfiguration configuration){

        //Add interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        //Add db context
        services.AddDbContext<IApplicationDbContext,StoreDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString(nameof(StoreDbContext)));
        });
        
        //Add repositories
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        
        return services;
    }
}
