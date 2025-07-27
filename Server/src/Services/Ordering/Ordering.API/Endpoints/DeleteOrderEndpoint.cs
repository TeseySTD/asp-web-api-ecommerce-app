using MediatR;
using Ordering.Application.UseCases.Orders.Commands.DeleteOrder;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;

namespace Ordering.API.Endpoints;

public class DeleteOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete($"/{{id:guid}}", async (ISender sender,Guid id) =>
        {
            var cmd = new DeleteOrderCommand(OrderId.Create(id).Value);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}