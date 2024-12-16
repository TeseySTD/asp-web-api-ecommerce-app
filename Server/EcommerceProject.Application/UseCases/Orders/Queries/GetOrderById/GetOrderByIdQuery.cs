using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(OrderId OrderId) : IQuery<OrderReadDto>;