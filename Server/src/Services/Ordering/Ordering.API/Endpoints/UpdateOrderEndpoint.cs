using System.Security.Claims;
using MediatR;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.UpdateOrder;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;

namespace Ordering.API.Endpoints;

public class UpdateOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut($"/{{id:guid}}", async (ISender sender, Guid id, ClaimsPrincipal user, UpdateOrderRequest request) =>
        {
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

            var customerIdString = user.FindFirstValue("userId");
            Guid customerId = Guid.TryParse(customerIdString, out Guid parsed) ? parsed : Guid.Empty;
            
            var cmd = new UpdateOrderCommand(customerId, id, dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).RequireAuthorization();
    }
}