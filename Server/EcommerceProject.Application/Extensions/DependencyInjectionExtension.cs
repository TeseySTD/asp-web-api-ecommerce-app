using System;
using System.Reflection;
using EcommerceProject.Application.Common.Classes.Behaviour;
using EcommerceProject.Application.Common.Classes.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
namespace EcommerceProject.Application.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services){
        services.AddMediatR( cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
