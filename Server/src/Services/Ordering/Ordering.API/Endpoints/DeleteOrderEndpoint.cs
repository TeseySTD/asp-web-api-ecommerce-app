using System.Security.Claims;
using MediatR;
using Ordering.Application.UseCases.Orders.Commands.DeleteOrder;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;

namespace Ordering.API.Endpoints;

public class DeleteOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete($"/{{id:guid}}", async (ISender sender, Guid id, ClaimsPrincipal userClaims) =>
        {
            if(ExtractUserDataFromClaims(userClaims).IsFailure)
                return Results.Unauthorized();
            var (customerId , userRole) = ExtractUserDataFromClaims(userClaims).Value;
            
            var cmd = new DeleteOrderCommand(OrderId.Create(id).Value, CustomerId.Create(customerId).Value, userRole);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors =>
                {
                    var enumerable = errors as Error[] ?? errors.ToArray();
                    if(enumerable.Any(e => e is DeleteOrderCommandHandler.CustomerMismatchError))
                        return Results.Forbid();
                    return Results.NotFound(Envelope.Of(enumerable));
                }
            );
        });
    }
}