using System.Security.Claims;
using Carter;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;

namespace Ordering.API.Endpoints;

public abstract class OrdersEndpoint : CarterModule
{
    public OrdersEndpoint() : base("/api/orders")
    {
        WithTags("Ordering");
        RequireAuthorization(Policies.RequireDefaultPolicy);
    }

    protected Result<(Guid customerId, UserRole userRole)> ExtractUserDataFromClaims(ClaimsPrincipal userClaims)
    {
        var customerIdString = userClaims.FindFirstValue("userId");
        var customerRoleString = userClaims.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(customerIdString) || string.IsNullOrEmpty(customerRoleString))
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid claims", "Missing required user claims"));
        }

        if (!Guid.TryParse(customerIdString, out Guid customerId) || customerId == Guid.Empty)
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid customer ID",
                "Customer ID claim is not a valid GUID"));
        }

        if (!Enum.TryParse(customerRoleString, out UserRole userRole))
        {
            return Result<(Guid, UserRole)>.Failure(new Error("Invalid role", "User role claim is not valid"));
        }

        return Result<(Guid, UserRole)>.Success((customerId, userRole));
    }
}