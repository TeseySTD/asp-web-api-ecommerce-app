using System.Security.Claims;
using MediatR;
using Ordering.Application.UseCases.Orders.Queries.GetOrders;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;
using Shared.Core.Auth;

namespace Ordering.API.Endpoints;

public class GetOrdersEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender,
            [AsParameters] PaginationRequest paginationRequest,
            Guid customerId,
            ClaimsPrincipal userClaims,
            string? orderStatus = null) =>
        {
            if (ExtractUserDataFromClaims(userClaims).IsFailure)
                return Results.Unauthorized();

            var (currentCustomerId, userRole) = ExtractUserDataFromClaims(userClaims).Value;
            if (currentCustomerId != customerId && userRole != UserRole.Admin)
                return Results.Forbid();

            OrderStatus? orderStatusEnum = null;
            if (orderStatus is not null && Enum.TryParse(orderStatus, ignoreCase: true, out OrderStatus parsedStatus))
                orderStatusEnum = parsedStatus;

            var query = new GetOrdersQuery(paginationRequest, CustomerId.Create(customerId).Value, orderStatusEnum);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}