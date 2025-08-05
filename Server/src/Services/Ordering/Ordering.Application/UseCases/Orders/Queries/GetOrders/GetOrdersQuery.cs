using Ordering.Application.Dto.Order;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.API;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrders;

public record GetOrdersQuery(PaginationRequest PaginationRequest, CustomerId CustomerId) : IQuery<PaginatedResult<OrderReadDto>>;