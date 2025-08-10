using Ordering.Application.Dto.Order;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Auth;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(OrderId OrderId, CustomerId CustomerId, UserRole CustomerRole) : IQuery<OrderReadDto>;