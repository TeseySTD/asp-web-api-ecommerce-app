using System.Reflection;
using FluentValidation;
using Mapster;
using MassTransit;
using Shared.Core.Behaviours;
using Shared.Messaging.Broker;

namespace Basket.API.Application;

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
        
        services.AddMessageBroker(configuration, "basket-api", configure =>
        {
            configure.AddConsumers(Assembly.GetExecutingAssembly());
            configure.SetInMemorySagaRepositoryProvider();
            configure.AddSagaStateMachines(Assembly.GetExecutingAssembly());
        });
        
        //Mapping
        services.AddMapster();
        MapsterConfig.Configure(services.BuildServiceProvider());
        
        return services;
    }
}