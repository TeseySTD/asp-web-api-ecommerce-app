using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Behaviours;
using Shared.Messaging.Broker;

namespace Ordering.Application;

public static class DependecyInjection
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR( cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());
        return services;
    }
    
}