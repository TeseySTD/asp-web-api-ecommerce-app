using Ordering.Application.Dto.Order;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrders;

public record GetOrdersQuery() : IQuery<IEnumerable<OrderReadDto>>;