using Ordering.Application.Dto.Order;
using Shared.Core.API;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrders;

public record GetOrdersQuery(PaginationRequest PaginationRequest) : IQuery<PaginatedResult<OrderReadDto>>;