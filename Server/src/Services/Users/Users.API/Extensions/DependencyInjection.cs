using Users.API.Helpers;
using Users.Application.Common.Interfaces;

namespace Users.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IEmailVerificationLinkFactory, EmailVerificationLinkFactory>();
        
        return services;
    }
}