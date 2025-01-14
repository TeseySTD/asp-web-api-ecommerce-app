using MediatR;
using Ordering.Application.UseCases.Orders.Queries.GetOrderById;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;

namespace Ordering.API.Endpoints;

public class GetByIdOrderEndpoint : OrdersEndpoint
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"/{{id:guid}}", async (ISender sender, Guid id) =>
        {
            var query = new GetOrderByIdQuery(OrderId.Create(id).Value);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}