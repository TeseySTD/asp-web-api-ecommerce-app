using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Common.Interfaces;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Helpers;

namespace Users.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddFluentEmail(configuration.GetSection("Email:SenderEmail").Value, configuration.GetSection("Email:Sender").Value)
            .AddSmtpSender(configuration.GetSection("Email:Host").Value, configuration.GetValue<int>("Email:Port"));
        
        services.AddTransient<IPasswordHelper, PasswordHelper>();
        services.AddTransient<ITokenProvider, TokenProvider>();
        services.AddTransient<IEmailHelper, EmailHelper>();
        
        return services;
    }
    
    
}