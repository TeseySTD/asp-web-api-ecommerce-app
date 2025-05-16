using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Messaging.Broker;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, string serviceName, Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(config =>
        {
            var formatter = new KebabCaseEndpointNameFormatter(serviceName, false);

            if (configure != null)
                configure.Invoke(config);
            
            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), hostConfigurator =>
                {
                    hostConfigurator.Username(configuration["MessageBroker:UserName"]!);
                    hostConfigurator.Password(configuration["MessageBroker:Password"]!);
                });
                configurator.UseInMemoryOutbox(context);

                configurator.ConfigureEndpoints(context, formatter);
            });
        });
        
        return services;
    }
}