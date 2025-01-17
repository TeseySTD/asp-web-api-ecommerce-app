﻿using Microsoft.Extensions.DependencyInjection;
using Users.Application.Common.Interfaces;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Helpers;

namespace Users.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHelper, PasswordHelper>();
        services.AddTransient<ITokenProvider, TokenProvider>();
        return services;
    }
    
    
}