using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.UseCases.Orders.EventHandlers.Integration;
using Ordering.Application.UseCases.Orders.Sagas;
using Ordering.Persistence.Interceptors;
using Shared.Messaging.Broker;

namespace Ordering.Persistence;

public static class DependecyInjection
{
    public static IServiceCollection AddPersistenceLayerServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        //Add interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        //Add db context
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });

        //Add sagas state.
        services.AddMessageBroker(configuration, configure =>
        {
            configure.AddConsumer<CanceledOrderEventHandler>();
            configure.AddConsumer<ApprovedOrderEventHandler>();
            configure.AddConsumer<ProductUpdatedEventHandler>();
            
            configure.AddSagaStateMachine<MakeOrderSaga, MakeOrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<ApplicationDbContext>();
                    r.UsePostgres();
                });
        });

        return services;
    }
}