using System.Security.Claims;
using MediatR;
using Ordering.Application.UseCases.Orders.Queries.GetOrderById;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;

namespace Ordering.API.Endpoints;

public class GetByIdOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"/{{id:guid}}", async (ISender sender, Guid id, ClaimsPrincipal userClaims) =>
        {
            if(ExtractUserDataFromClaims(userClaims).IsFailure)
                return Results.Unauthorized();
            var (customerId , userRole) = ExtractUserDataFromClaims(userClaims).Value;

            var query = new GetOrderByIdQuery(OrderId.Create(id).Value, CustomerId.Create(customerId).Value, userRole);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors =>
                {
                    var enumerable = errors as Error[] ?? errors.ToArray();
                    if (enumerable.Any(e => e is GetOrderByIdQueryHandler.CustomerMismatchError))
                        return Results.Forbid();
                    return Results.NotFound(Envelope.Of(enumerable));
                });
        });
    }
}