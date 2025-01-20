using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Core.Auth;

public static class Authorization
{
    public static IServiceCollection AddAuthrorizationWithRoleHierarchyPolicies(this IServiceCollection services,
        Action<AuthorizationOptions>? configure = null)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireAdminPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, Policies.RequireAdminPolicy);
                })
            );

            options.AddPolicy(Policies.RequireSellerPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, Policies.RequireSellerPolicy);
                })
            );

            options.AddPolicy(Policies.RequireDefaultPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, Policies.RequireDefaultPolicy);
                })
            );
            
            if(configure != null)
                services.Configure(configure);
        });
        
        return services;
    }
}