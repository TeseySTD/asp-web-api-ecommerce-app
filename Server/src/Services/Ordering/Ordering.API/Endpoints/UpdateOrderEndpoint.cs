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
        app.MapPut($"/{{id:guid}}", async (ISender sender,Guid id, UpdateOrderRequest request) =>
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

            var dto = new OrderUpdateDto(
                OrderItems: orderItems,
                Payment: payment,
                DestinationAddress: address
            );

            var cmd = new UpdateOrderCommand(OrderId.Create(id).Value, dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });
    }
}