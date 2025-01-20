using MediatR;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Shared.Core.API;

namespace Ordering.API.Endpoints;

public class MakeOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (ISender sender, MakeOrderRequest request) =>
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

            var orderItems = request.OrderItems.Select(i =>
                (i.ProductId, i.ProductName, i.ProductDescription, i.Quantity, i.Price)
            );

            var dto = new OrderWriteDto(
                UserId: request.UserId,
                OrderItems: orderItems,
                Payment: payment,
                DestinationAddress: address
            );

            var cmd = new CreateOrderCommand(dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        }).RequireAuthorization();
    }
}