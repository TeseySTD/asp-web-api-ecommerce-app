using System.Security.Claims;
using MediatR;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.UpdateOrder;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;
using Shared.Core.Validation.Result;

namespace Ordering.API.Endpoints;

public class UpdateOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut($"/{{id:guid}}", async (ISender sender, Guid id, ClaimsPrincipal userClaims, UpdateOrderRequest request) =>
        {
            if(ExtractUserDataFromClaims(userClaims).IsFailure)
                return Results.Unauthorized();
            var customerId = ExtractUserDataFromClaims(userClaims).Value.customerId;
            
            var payment = (
                request.CardName,
                request.CardNumber,
                request.Expiration,
                request.CVV,
                request.PaymentMethod
            );

            var address = (
                request.AddressLine,
                request.Country,
                request.State,
                request.ZipCode
            );

            var dto = new OrderUpdateDto(
                Payment: payment,
                DestinationAddress: address
            );

            var cmd = new UpdateOrderCommand(customerId, id, dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors =>
                {
                    var enumerable = errors as Error[] ?? errors.ToArray();
                    
                    if(enumerable.Any(e => e is UpdateOrderCommandHandler.OrderNotFoundError))
                        return Results.NotFound(Envelope.Of(enumerable));
                    else if (enumerable.Any(e => e is UpdateOrderCommandHandler.CustomerMismatchError))
                        return Results.Forbid();
                    return Results.BadRequest(Envelope.Of(enumerable));
                });
        });
    }
}