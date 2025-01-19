using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Messaging.Broker;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly is not null)
            {
                config.AddConsumers(assembly);
                config.SetInMemorySagaRepositoryProvider();
                config.AddSagaStateMachines(assembly);
            }
            
            
            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), hostConfigurator =>
                {
                    hostConfigurator.Username(configuration["MessageBroker:UserName"]!);
                    hostConfigurator.Password(configuration["MessageBroker:Password"]!);
                });
                configurator.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}