using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Order;

namespace EcommerceProject.Application.UseCases.Orders.Queries.GetOrders;

public record GetOrdersQuery() : IQuery<IEnumerable<OrderReadDto>>;