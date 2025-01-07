using EcommerceProject.Core.Models.Users;

namespace EcommerceProject.Infrastructure.Authorization;

public static class RoleHierarchy
{
    private static readonly Dictionary<string, int> RoleLevels = new ()
    {
        { User.UserRole.Admin.ToString(), 3 },
        { User.UserRole.Seller.ToString(), 2 },
        { User.UserRole.Default.ToString(), 1 }
    };

    public static bool IsRoleAllowed(string? userRole, string requiredRole)
    {
        if (userRole == null || !RoleLevels.ContainsKey(userRole) || !RoleLevels.ContainsKey(requiredRole))
            return false;

        return RoleLevels[userRole] >= RoleLevels[requiredRole];
    }
}