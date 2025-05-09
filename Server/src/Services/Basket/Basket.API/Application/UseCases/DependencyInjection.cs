using System.Reflection;
using FluentValidation;
using MassTransit;
using Shared.Core.Behaviours;
using Shared.Messaging.Broker;

namespace Basket.API.Application.UseCases;

public static class DependencyInjection
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
        
        services.AddMessageBroker(configuration, configure =>
        {
            configure.AddConsumers(Assembly.GetExecutingAssembly());
            configure.SetInMemorySagaRepositoryProvider();
            configure.AddSagaStateMachines(Assembly.GetExecutingAssembly());
        });
        
        return services;
    }
}