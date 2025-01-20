namespace Shared.Core.Auth;

public static class RoleHierarchy
{
    
    private static readonly Dictionary<string, int> RoleLevels = new ()
    {
        { UserRole.Admin.ToString(), 3 },
        { UserRole.Seller.ToString(), 2 },
        { UserRole.Default.ToString(), 1 }
    };

    public static bool IsRoleAllowed(string? userRole, string requiredRole)
    {
        if (userRole == null || !RoleLevels.ContainsKey(userRole) || !RoleLevels.ContainsKey(requiredRole))
            return false;

        return RoleLevels[userRole] >= RoleLevels[requiredRole];
    }
}