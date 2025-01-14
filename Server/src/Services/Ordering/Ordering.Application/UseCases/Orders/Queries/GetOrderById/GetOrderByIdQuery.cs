using Ordering.Application.Dto.Order;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(OrderId OrderId) : IQuery<OrderReadDto>;