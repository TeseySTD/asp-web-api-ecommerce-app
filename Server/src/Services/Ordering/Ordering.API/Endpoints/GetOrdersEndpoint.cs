﻿using MediatR;
using Ordering.Application.UseCases.Orders.Queries.GetOrders;
using Shared.Core.API;

namespace Ordering.API.Endpoints;

public class GetOrdersEndpoint : OrdersEndpoint
{

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ISender sender) =>
        {
            var query = new GetOrdersQuery();
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}