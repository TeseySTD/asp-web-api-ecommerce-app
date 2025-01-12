using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Users.Core.Models;
using Users.Infrastructure.Authorization;

namespace Users.Infrastructure.Extensions;

public static class Authorization
{
    public static IServiceCollection AddAuthrorizationWithRoleHierarchyPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireAdminPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, User.UserRole.Admin.ToString());
                })
            );
            
            options.AddPolicy(Policies.RequireSellerPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, User.UserRole.Seller.ToString());
                })
            );
            
            options.AddPolicy(Policies.RequireDefaultPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    return RoleHierarchy.IsRoleAllowed(userRole, User.UserRole.Default.ToString());
                })
            );
        });

        return services;
    }
}