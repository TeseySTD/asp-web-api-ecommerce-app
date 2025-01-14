using Carter;
using MediatR;

namespace Ordering.API.Endpoints;

public abstract class OrdersEndpoint : CarterModule
{
    public OrdersEndpoint() : base("/api/orders")
    {
        
    }
}