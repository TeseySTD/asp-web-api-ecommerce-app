using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceProject.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHelper, PasswordHelper>();
        return services;
    }
    
    
}