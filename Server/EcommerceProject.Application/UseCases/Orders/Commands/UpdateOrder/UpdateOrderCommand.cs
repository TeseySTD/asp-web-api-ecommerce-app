using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(OrderId OrderId, OrderUpdateDto Value) : ICommand;